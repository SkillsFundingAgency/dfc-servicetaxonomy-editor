using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class ValidateAndRepairGraph : IValidateAndRepairGraph
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IGraphClusterLowLevel _graphClusterLowLevel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IGraphValidationHelper _graphValidationHelper;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly ILogger<ValidateAndRepairGraph> _logger;
        private IGraph? _currentGraph;

        public ValidateAndRepairGraph(IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session,
            IServiceProvider serviceProvider,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IContentItemVersionFactory contentItemVersionFactory,
            ILogger<ValidateAndRepairGraph> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _serviceProvider = serviceProvider;
            _graphSyncHelper = graphSyncHelper;
            _graphValidationHelper = graphValidationHelper;
            _contentItemVersionFactory = contentItemVersionFactory;
            _logger = logger;
            _currentGraph = default;

            _graphClusterLowLevel = _serviceProvider.GetRequiredService<IGraphClusterLowLevel>();
        }

        public async Task<ValidateAndRepairResults> ValidateGraph(params string[] graphReplicaSetNames)
        {
            IEnumerable<ContentTypeDefinition> syncableContentTypeDefinitions = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)));

            DateTime timestamp = DateTime.UtcNow;
            AuditSyncLog auditSyncLog = await _session.Query<AuditSyncLog>().FirstOrDefaultAsync() ??
                                        new AuditSyncLog {LastSynced = SqlDateTime.MinValue.Value};

            IEnumerable<string> graphReplicaSetNamesToValidate = graphReplicaSetNames.Any()
                ? graphReplicaSetNames
                : _graphClusterLowLevel.GraphReplicaSetNames;

            var results = new ValidateAndRepairResults(auditSyncLog.LastSynced);

            //todo: we could optimise to only get content items from the oc database once for each replica set,
            // rather than for each instance
            foreach (string graphReplicaSetName in graphReplicaSetNamesToValidate)
            {
                IGraphReplicaSetLowLevel graphReplicaSetLowLevel = _graphClusterLowLevel.GetGraphReplicaSetLowLevel(graphReplicaSetName);
                IContentItemVersion contentItemVersion = _contentItemVersionFactory.Get(graphReplicaSetName);

                foreach (IGraph graph in graphReplicaSetLowLevel.GraphInstances)
                {
                    ValidateAndRepairResult result = results.AddNewValidateAndRepairResult(
                        graphReplicaSetName,
                        graph.Instance,
                        graph.Endpoint.Name,
                        graph.GraphName,
                        graph.DefaultGraph);

                    // make current graph available for when parts/fields call back into ValidateContentItem
                    // seems a little messy, and will make concurrent validation a pita,
                    // but stops part/field syncers needing low level graph access
                    _currentGraph = graph;

                    foreach (ContentTypeDefinition contentTypeDefinition in syncableContentTypeDefinitions)
                    {
                        List<ValidationFailure> syncFailures = await ValidateContentItemsOfContentType(
                            contentItemVersion,
                            contentTypeDefinition,
                            auditSyncLog.LastSynced,
                            result);
                        if (syncFailures.Any())
                        {
                            await AttemptRepair(syncFailures, contentTypeDefinition, contentItemVersion, result);
                        }
                    }
                }
            }

            if (results.AnyRepairFailures)
                return results;

            _logger.LogInformation("Woohoo: graph passed validation or was successfully repaired.");

            auditSyncLog.LastSynced = timestamp;
            _session.Save(auditSyncLog);
            await _session.CommitAsync();

            return results;
        }

        private async Task<List<ValidationFailure>> ValidateContentItemsOfContentType(
            IContentItemVersion contentItemVersion,
            ContentTypeDefinition contentTypeDefinition,
            DateTime lastSynced,
            ValidateAndRepairResult result)
        {
            List<ValidationFailure> syncFailures = new List<ValidationFailure>();

            //todo: do we want to batch up content items of type?
            IEnumerable<ContentItem> contentTypeContentItems = await GetContentItems(
                contentItemVersion, contentTypeDefinition, lastSynced);

            if (!contentTypeContentItems.Any())
            {
                _logger.LogDebug($"No {contentTypeDefinition.Name} content items found that require validation.");
                return syncFailures;
            }

            foreach (ContentItem contentItem in contentTypeContentItems)
            {
                (bool validated, string? validationFailureReason) =
                    await ValidateContentItem(contentItem, contentTypeDefinition, contentItemVersion);

                if (validated)
                {
                    _logger.LogInformation($"Sync validation passed for {contentItem.ContentType} {contentItem.ContentItemId} in {GraphDescription(_currentGraph!)}.");
                    result.Validated.Add(contentItem);
                }
                else
                {
                    _logger.LogWarning($"Sync validation failed in {GraphDescription(_currentGraph!)}.{Environment.NewLine}{validationFailureReason}");
                    ValidationFailure validationFailure = new ValidationFailure(contentItem, validationFailureReason!);
                    syncFailures.Add(validationFailure);
                    result.ValidationFailures.Add(validationFailure);
                }
            }

            return syncFailures;
        }

        private async Task<IEnumerable<ContentItem>> GetContentItems(
            IContentItemVersion contentItemVersion,
            ContentTypeDefinition contentTypeDefinition,
            DateTime lastSynced)
        {
            (bool? latest, bool? published) = contentItemVersion.ContentItemIndexFilterTerms;

            return await _session
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == contentTypeDefinition.Name
                    && (latest == null || x.Latest == latest)
                    && (published == null || x.Published == published)
                    && (x.CreatedUtc >= lastSynced || x.ModifiedUtc >= lastSynced))
                .ListAsync();
        }

        //todo: ToString in Graph?
        private string GraphDescription(IGraph graph)
        {
            return $"graph instance #{graph.Instance} in replica set {graph.GraphReplicaSetLowLevel.Name}";
        }

        private async Task AttemptRepair(
            IEnumerable<ValidationFailure> syncValidationFailures,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion,
            ValidateAndRepairResult result)
        {
            _logger.LogWarning(
                $"Content items of type {contentTypeDefinition.Name} failed validation ({string.Join(", ", syncValidationFailures.Select(f => f.ContentItem.ToString()))}). Attempting to repair them.");

            // if this throws should we carry on?
            foreach (var failure in syncValidationFailures)
            {
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                IGraphReplicaSet graphReplicaSet = _currentGraph!.GetReplicaSetLimitedToThisGraph();
                await mergeGraphSyncer.SyncToGraphReplicaSet(graphReplicaSet, failure.ContentItem, _contentManager);

                //todo: split into smaller methods

                (bool validated, string? validationFailureReason) =
                    await ValidateContentItem(failure.ContentItem, contentTypeDefinition, contentItemVersion);

                if (validated)
                {
                    _logger.LogInformation($"Repair was successful on {failure.ContentItem.ContentType} {failure.ContentItem.ContentItemId} in {GraphDescription(_currentGraph!)}.");
                    result.Repaired.Add(failure.ContentItem);
                }
                else
                {
                    _logger.LogWarning($"Repair was unsuccessful.{Environment.NewLine}{validationFailureReason}");
                    result.RepairFailures.Add(new RepairFailure(failure.ContentItem, validationFailureReason!));
                }
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion)
        {
            _logger.LogDebug($"Validating {contentItem.ContentType} {contentItem.ContentItemId} '{contentItem.DisplayText}'");

            _graphSyncHelper.ContentType = contentItem.ContentType;

            object nodeId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            List<INodeWithOutgoingRelationships?> results = await _currentGraph!.Run(
                new NodeWithOutgoingRelationshipsQuery(
                    await _graphSyncHelper.NodeLabels(),
                    _graphSyncHelper.IdPropertyName(),
                    nodeId));

            INodeWithOutgoingRelationships? nodeWithOutgoingRelationships = results.FirstOrDefault();
            if (nodeWithOutgoingRelationships == null)
                return (false, FailureContext("Node not found.", contentItem));

            ValidateAndRepairItemSyncContext context = new ValidateAndRepairItemSyncContext(
                _contentManager,
                contentItemVersion,
                nodeWithOutgoingRelationships,
                _graphSyncHelper,
                _graphValidationHelper,
                this,
                contentTypeDefinition,
                nodeId);

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not?
                if (itemSyncer.CanSync(contentItem))
                {
                    (bool validated, string failureReason) =
                        await itemSyncer.ValidateSyncComponent(contentItem, context);
                    if (!validated)
                        return (validated, failureReason);
                }
            }

            // check there aren't any more relationships of each type than there should be
            // we need to do this after all parts have added their own expected relationship counts
            // to handle different parts or multiple named instances of a part creating relationships of the same type
            foreach ((string relationshipType, int relationshipsInDbCount) in context.ExpectedRelationshipCounts)
            {
                int relationshipsInGraphCount =
                    nodeWithOutgoingRelationships.OutgoingRelationships.Count(r =>
                        r.Relationship.Type == relationshipType);

                if (relationshipsInDbCount != relationshipsInGraphCount)
                {
                    return (false, FailureContext(
                        $"Expecting {relationshipsInDbCount} relationships of type {relationshipType} in graph, but found {relationshipsInGraphCount}.",
                        contentItem));
                }
            }

            return (true, "");
        }

        public string FailureContext(
            string failureReason,
            ContentItem contentItem)
        {
            return $@"{failureReason}
Content ----------------------------------------
          type: '{contentItem.ContentType}'
            ID: {contentItem.ContentItemId}";
        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
