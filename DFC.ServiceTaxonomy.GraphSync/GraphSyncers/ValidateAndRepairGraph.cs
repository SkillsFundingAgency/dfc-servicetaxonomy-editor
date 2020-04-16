using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class ValidateAndRepairGraph : IValidateAndRepairGraph
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IGraphValidationHelper _graphValidationHelper;
        private readonly ILogger<ValidateAndRepairGraph> _logger;
        private readonly Dictionary<string, IContentPartGraphSyncer> _partSyncers;

        public ValidateAndRepairGraph(
            IContentDefinitionManager contentDefinitionManager,
            ISession session,
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            ILogger<ValidateAndRepairGraph> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _graphSyncHelper = graphSyncHelper;
            _graphValidationHelper = graphValidationHelper;
            _logger = logger;
            _partSyncers = partSyncers.ToDictionary(x => x.PartName ?? "Eponymous");
        }

        public async Task<ValidateAndRepairResult> ValidateGraph()
        {
            IEnumerable<ContentTypeDefinition> syncableContentTypeDefinitions = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)));

            DateTime timestamp = DateTime.UtcNow;
            AuditSyncLog auditSyncLog = await _session.Query<AuditSyncLog>().FirstOrDefaultAsync() ??
                                        new AuditSyncLog {LastSynced = SqlDateTime.MinValue.Value};

            ValidateAndRepairResult result = new ValidateAndRepairResult(auditSyncLog.LastSynced);

            foreach (ContentTypeDefinition contentTypeDefinition in syncableContentTypeDefinitions)
            {
                List<ValidationFailure> syncFailures = await ValidateContentItemsOfContentType(contentTypeDefinition, auditSyncLog.LastSynced, result);
                if (syncFailures.Any())
                {
                    await AttemptRepair(syncFailures.Select(f => f.ContentItem), contentTypeDefinition, result);
                }
            }

            if (!result.RepairFailures.Any())
            {
                _logger.LogInformation("Woohoo: graph passed validation or was successfully repaired.");

                auditSyncLog.LastSynced = timestamp;
                _session.Save(auditSyncLog);
                await _session.CommitAsync();
            }

            return result;
        }

        private async Task<List<ValidationFailure>> ValidateContentItemsOfContentType(
            ContentTypeDefinition contentTypeDefinition,
            DateTime lastSynced,
            ValidateAndRepairResult result)
        {
            List<ValidationFailure> syncFailures = new List<ValidationFailure>();

            //todo: do we want to batch up content items of type?
            IEnumerable<ContentItem> contentTypeContentItems = await _session
                //do we only care about the latest published items?
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == contentTypeDefinition.Name
                    && x.Latest && x.Published
                    && (x.CreatedUtc >= lastSynced || x.ModifiedUtc >= lastSynced))
                .ListAsync();

            if (!contentTypeContentItems.Any())
            {
                _logger.LogDebug($"No {contentTypeDefinition.Name} content items found that require validation.");
                return syncFailures;
            }

            foreach (ContentItem contentItem in contentTypeContentItems)
            {
                (bool validated, string? validationFailureReason) =
                    await ValidateContentItem(contentItem, contentTypeDefinition);
                if (validated)
                {
                    _logger.LogInformation($"Sync validation passed for {contentItem.ContentType} {contentItem.ContentItemId}.");
                    result.Validated.Add(contentItem);
                }
                else
                {
                    _logger.LogWarning($"Sync validation failed.{Environment.NewLine}{validationFailureReason}");
                    ValidationFailure validationFailure = new ValidationFailure(contentItem, validationFailureReason!);
                    syncFailures.Add(validationFailure);
                    result.ValidationFailures.Add(validationFailure);
                }
            }

            return syncFailures;
        }

        private async Task AttemptRepair(
            IEnumerable<ContentItem> syncFailedContentItems,
            ContentTypeDefinition contentTypeDefinition,
            ValidateAndRepairResult result)
        {
            _logger.LogWarning(
                $"Content items of type {contentTypeDefinition.Name} failed validation ({string.Join(", ", syncFailedContentItems.Select(ci => ci.ToString()))}).Attempting to repair them.");

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

                //todo: split into smaller methods

                (bool validated, string? validationFailureReason) =
                    await ValidateContentItem(failedSyncContentItem, contentTypeDefinition);
                if (validated)
                {
                    _logger.LogInformation("Repair was successful on {contentItem.ContentType} {contentItem.ContentItemId}.");
                    result.Repaired.Add(failedSyncContentItem);
                }
                else
                {
                    _logger.LogWarning($"Repair was unsuccessful.{Environment.NewLine}{validationFailureReason}");
                    result.RepairFailures.Add(new RepairFailure(failedSyncContentItem, validationFailureReason!));
                }
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateContentItem(ContentItem contentItem, ContentTypeDefinition contentTypeDefinition)
        {
            _logger.LogDebug($"Validating {contentItem.ContentType} {contentItem.ContentItemId} '{contentItem.DisplayText}'");

            _graphSyncHelper.ContentType = contentItem.ContentType;

            object nodeId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart);

            List<IRecord> results = await _graphDatabase.Run(new MatchNodeWithAllOutgoingRelationshipsQuery(
                await _graphSyncHelper.NodeLabels(),
                _graphSyncHelper.IdPropertyName(),
                nodeId));

            //todo: these need more context
            if (results == null || !results.Any())
                return (false, "Node missing from graph (no results).");

            INode? sourceNode = results.Select(x => x[0]).Cast<INode?>().FirstOrDefault();
            if (sourceNode == null)
                return (false, "Node missing from graph.");

            List<IRelationship> relationships = results.Select(x => x[1]).Cast<IRelationship>().ToList();
            List<INode> destinationNodes = results.Select(x => x[2]).Cast<INode>().ToList();

            //todo: for some reason sometimes we get an array with a single null element
            relationships.RemoveAll(x => x == null);

            IDictionary<string, int> expectedRelationshipCounts = new Dictionary<string, int>();

            foreach (ContentTypePartDefinition contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                string partTypeName = contentTypePartDefinition.PartDefinition.Name;
                string partName = contentTypePartDefinition.Name;
                if (!_partSyncers.TryGetValue(partTypeName, out var partSyncer)
                    && partName == contentTypePartDefinition.ContentTypeDefinition.DisplayName)
                {
                    partSyncer = _partSyncers["Eponymous"];
                }

                if (partSyncer == null)
                {
                    // part doesn't have a registered IContentPartGraphSyncer, so we ignore it
                    continue;
                }

                dynamic? partContent = contentItem.Content[partName];
                if (partContent == null)
                    continue; //todo: throw??

                (bool verified, string partFailureReason) = await partSyncer.VerifySyncComponent(
                    (JObject)partContent, contentTypePartDefinition,
                    sourceNode, relationships, destinationNodes,
                    _graphSyncHelper, _graphValidationHelper,
                    expectedRelationshipCounts);

                if (!verified)
                {
                    string failureReason = $"{partSyncer.PartName} did not validate: {partFailureReason}";
                    string failureContext = FailureContext(failureReason, nodeId, contentTypePartDefinition, contentItem, partTypeName, partContent, sourceNode);
                    return (false, failureContext);
                }
            }

            // check there aren't any more relationships of each type than there should be
            // we need to do this after all parts have added their own expected relationship counts
            // to handle different parts or multiple named instances of a part creating relationships of the same type
            foreach ((string relationshipType, int relationshipsInDbCount) in expectedRelationshipCounts)
            {
                int relationshipsInGraphCount = relationships.Count(r => r.Type == relationshipType);

                if (relationshipsInDbCount != relationshipsInGraphCount)
                {
                    //todo: needs more context
                    return (false, $"Expecting {relationshipsInDbCount} relationships of type {relationshipType} in graph, but found {relationshipsInGraphCount}.");
                }
            }

            return (true, "");
        }

        //todo: for bag failures, source node is containing type, which isn't much use,
        private string FailureContext(
            string failureReason,
            object nodeId,
            ContentTypePartDefinition contentTypePartDefinition,
            ContentItem contentItem,
            string partName,
            dynamic partContent,
            INode sourceNode)
        {
            return $@"{failureReason}
Content ----------------------------------------
          type: '{contentItem.ContentType}'
            ID: {contentItem.ContentItemId}
part type name: '{partName}'
     part name: '{contentTypePartDefinition.Name}'
  part content:
{partContent}
Source Node ------------------------------------
        ID: {sourceNode.Id}
   user ID: {nodeId}
    labels: ':{string.Join(":", sourceNode.Labels)}'
properties:
{string.Join(Environment.NewLine, sourceNode.Properties.Select(p => $"{p.Key} = {p.Value}"))}";
        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
