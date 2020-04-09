using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class ValidateGraphSync : IValidateGraphSync
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IGraphValidationHelper _graphValidationHelper;
        private readonly ILogger<ValidateGraphSync> _logger;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;
        private readonly Dictionary<string, IContentPartGraphSyncer> _partSyncers;

        public ValidateGraphSync(
            IContentDefinitionManager contentDefinitionManager,
            ISession session,
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            ILogger<ValidateGraphSync> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _graphSyncHelper = graphSyncHelper;
            _graphValidationHelper = graphValidationHelper;
            _logger = logger;
            _partSyncers = partSyncers.ToDictionary(x => x.PartName ?? "Eponymous");
            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        public async Task<bool> ValidateGraph()
        {
            bool validatedOk = true;

            IEnumerable<ContentTypeDefinition> syncableContentTypeDefinitions = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)));

            DateTime timestamp = DateTime.UtcNow;
            AuditSyncLog auditSyncLog = await _session.Query<AuditSyncLog>().FirstOrDefaultAsync() ??
                                        new AuditSyncLog {LastSynced = SqlDateTime.MinValue.Value};

            foreach (ContentTypeDefinition contentTypeDefinition in syncableContentTypeDefinitions)
            {
                //todo: do we want to batch up content items of type?
                IEnumerable<ContentItem> contentTypeContentItems = await _session
                    //do we only care about the latest published items?
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == contentTypeDefinition.Name
                        && x.Latest && x.Published
                        && (x.CreatedUtc >= auditSyncLog.LastSynced || x.ModifiedUtc >= auditSyncLog.LastSynced))
                    .ListAsync();

                if (!contentTypeContentItems.Any())
                    continue;

                List<ContentItem> syncFailedContentItems = new List<ContentItem>();

                foreach (ContentItem contentItem in contentTypeContentItems)
                {
                    //todo: do we need a new _validateGraphSync each time? don't think we do
                    if (!await CheckIfContentItemSynced(contentItem, contentTypeDefinition))
                    {
                        syncFailedContentItems.Add(contentItem);
                    }
                }

                if (!syncFailedContentItems.Any())
                    continue;

                validatedOk = false;

                _logger.LogWarning(
                    $"Content items of type {contentTypeDefinition.Name} failed validation ({string.Join(", ", syncFailedContentItems.Select(ci => ci.ToString()))}). Attempting to resync them");

                // if this throws should we carry on?
                foreach (ContentItem failedSyncContentItem in syncFailedContentItems)
                {
                    var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                    await mergeGraphSyncer.SyncToGraph(
                        failedSyncContentItem.ContentType,
                        failedSyncContentItem.ContentItemId,
                        failedSyncContentItem.ContentItemVersionId,
                        failedSyncContentItem.Content,
                        failedSyncContentItem.CreatedUtc,
                        failedSyncContentItem.ModifiedUtc);

                    // do we want to double check sync was ok?
                    //if (!await _validateGraphSync.CheckIfContentItemSynced(contentItem))
                }
            }

            auditSyncLog.LastSynced = timestamp;
            _session.Save(auditSyncLog);
            await _session.CommitAsync();

            if (validatedOk)
            {
                _logger.LogInformation("Woohoo: graph passed validation.");
            }

            return validatedOk;
        }

        public async Task<bool> CheckIfContentItemSynced(ContentItem contentItem, ContentTypeDefinition contentTypeDefinition)
        {
            _graphSyncHelper.ContentType = contentItem.ContentType;

            object nodeId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart);

            List<IRecord> results = await _graphDatabase.Run(new MatchNodeWithAllOutgoingRelationshipsQuery(
                await _graphSyncHelper.NodeLabels(),
                _graphSyncHelper.IdPropertyName(),
                nodeId));

            if (results == null || !results.Any())
                return false;

            INode? sourceNode = results.Select(x => x[0]).Cast<INode?>().FirstOrDefault();
            if (sourceNode == null)
                return false;

            List<IRelationship> relationships = results.Select(x => x[1]).Cast<IRelationship>().ToList();
            List<INode> destinationNodes = results.Select(x => x[2]).Cast<INode>().ToList();

            //todo: for some reason sometimes we get an array with a single null element
            relationships.RemoveAll(x => x == null);

            foreach (ContentTypePartDefinition contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                string partName = contentTypePartDefinition.PartDefinition.Name;
                if (!_partSyncers.TryGetValue(partName, out var partSyncer))
                {
                    partSyncer = _partSyncers["Eponymous"];
                }

                dynamic? partContent = contentItem.Content[partName];
                if (partContent == null)
                    continue; //todo: throw??

                if (!await partSyncer.VerifySyncComponent(
                    partContent, contentTypePartDefinition,
                    sourceNode, relationships, destinationNodes,
                    _graphSyncHelper, _graphValidationHelper))
                {
                    LogValidationFailure(nodeId, contentTypePartDefinition, contentItem, partName, partContent, sourceNode);
                    return false;
                }
            }

            return true;
        }

        private void LogValidationFailure(
            object nodeId,
            ContentTypePartDefinition contentTypePartDefinition,
            ContentItem contentItem,
            string partName,
            dynamic partContent,
            INode sourceNode)
        {
            //todo: check these
            _logger.LogWarning($@"Sync validation failed.
Content type: '{contentItem.ContentType}'
Node ID: '{nodeId}'
Content part type name: '{contentTypePartDefinition.Name}'
             name '{partName}'
             content: '{partContent}'
Source node ID: {sourceNode.Id}
            labels: ':{string.Join(":", sourceNode.Labels)}'
            properties: '{string.Join(",", sourceNode.Properties.Select(p => $"{p.Key}={p.Value}"))}'");
        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
