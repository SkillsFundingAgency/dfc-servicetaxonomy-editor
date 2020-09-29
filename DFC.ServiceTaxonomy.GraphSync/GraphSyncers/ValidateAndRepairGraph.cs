using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public enum ValidationScope
    {
        ModifiedSinceLastValidation,
        AllItems
    }

    public class ValidateAndRepairGraph : IValidateAndRepairGraph
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IGraphClusterLowLevel _graphClusterLowLevel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IGraphValidationHelper _graphValidationHelper;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly IContentItemsService _contentItemsService;
        private readonly ILogger<ValidateAndRepairGraph> _logger;
        private IGraph? _currentGraph;
        private static readonly SemaphoreSlim _serialValidation = new SemaphoreSlim(1, 1);

        public ValidateAndRepairGraph(IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session,
            IServiceProvider serviceProvider,
            ISyncNameProvider syncNameProvider,
            IGraphValidationHelper graphValidationHelper,
            IContentItemVersionFactory contentItemVersionFactory,
            IContentItemsService contentItemsService,
            ILogger<ValidateAndRepairGraph> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _serviceProvider = serviceProvider;
            _syncNameProvider = syncNameProvider;
            _graphValidationHelper = graphValidationHelper;
            _contentItemVersionFactory = contentItemVersionFactory;
            _contentItemsService = contentItemsService;
            _logger = logger;
            _currentGraph = default;

            _graphClusterLowLevel = _serviceProvider.GetRequiredService<IGraphClusterLowLevel>();
        }

        public async Task<IValidateAndRepairResults> ValidateGraph(
            ValidationScope validationScope,
            params string[] graphReplicaSetNames)
        {
            if (!await _serialValidation.WaitAsync(TimeSpan.Zero))
                return ValidationAlreadyInProgressResult.Instance;

            try
            {
                return await ValidateGraphImpl(validationScope, graphReplicaSetNames);
            }
            finally
            {
                _serialValidation.Release();
            }
        }

        private async Task<IValidateAndRepairResults> ValidateGraphImpl(
            ValidationScope validationScope,
            params string[] graphReplicaSetNames)
        {
            IEnumerable<ContentTypeDefinition> syncableContentTypeDefinitions = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)));

            DateTime timestamp = DateTime.UtcNow;

            IEnumerable<string> graphReplicaSetNamesToValidate = graphReplicaSetNames.Any()
                ? graphReplicaSetNames
                : _graphClusterLowLevel.GraphReplicaSetNames;

            DateTime validateFrom = await GetValidateFromTime(validationScope);
            var results = new ValidateAndRepairResults(validateFrom);

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
                            validateFrom,
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

            _session.Save(new AuditSyncLog(timestamp));
            await _session.CommitAsync();

            return results;
        }

        private async Task<DateTime> GetValidateFromTime(ValidationScope validationScope)
        {
            if (validationScope == ValidationScope.AllItems)
                return SqlDateTime.MinValue.Value;

            var auditSyncLog = await _session
                .Query<AuditSyncLog>()
                .ToAsyncEnumerable()
                .LastOrDefaultAsync();
            return auditSyncLog?.LastSynced ?? SqlDateTime.MinValue.Value;
        }

        private async Task<List<ValidationFailure>> ValidateContentItemsOfContentType(
            IContentItemVersion contentItemVersion,
            ContentTypeDefinition contentTypeDefinition,
            DateTime lastSynced,
            ValidateAndRepairResult result)
        {
            List<ValidationFailure> syncFailures = new List<ValidationFailure>();

            (bool? latest, bool? published) = contentItemVersion.ContentItemIndexFilterTerms;

            //todo: do we want to batch up content items of type?
            IEnumerable<ContentItem> contentTypeContentItems = await _contentItemsService
                .Get(contentTypeDefinition.Name, lastSynced, latest: latest, published: published);

            IEnumerable<ContentItem> deletedContentTypeContentItems = await _contentItemsService
                .GetDeleted(contentTypeDefinition.Name, lastSynced);

            if (!contentTypeContentItems.Any() && !deletedContentTypeContentItems.Any())
            {
                _logger.LogDebug("No {ContentType} content items found that require validation.", contentTypeDefinition.Name);
                return syncFailures;
            }

            foreach (ContentItem contentItem in contentTypeContentItems)
            {
                (bool validated, string? validationFailureReason) =
                    await ValidateContentItem(contentItem, contentTypeDefinition, contentItemVersion);

                if (validated)
                {
                    _logger.LogInformation("Sync validation passed for {ContentType} {ContentItemId} in {CurrentGraph}.",
                        contentItem.ContentType,
                        contentItem.ContentItemId,
                        GraphDescription(_currentGraph!));
                    result.Validated.Add(contentItem);
                }
                else
                {
                    string message = $"Sync validation failed in {{CurrentGraph}}.{Environment.NewLine}{{ValidationFailureReason}}.";
                    _logger.LogWarning(message, GraphDescription(_currentGraph!), validationFailureReason);
                    ValidationFailure validationFailure = new ValidationFailure(contentItem, validationFailureReason!);
                    syncFailures.Add(validationFailure);
                    result.ValidationFailures.Add(validationFailure);
                }
            }

            foreach (ContentItem contentItem in deletedContentTypeContentItems)
            {
                (bool validated, string? validationFailureReason) =
                    await ValidateDeletedContentItem(contentItem, contentTypeDefinition, contentItemVersion);

                if (validated)
                {
                    _logger.LogInformation("Sync validation passed for deleted {ContentType} {ContentItemId} in {CurrentGraph}.",
                        contentItem.ContentType,
                        contentItem.ContentItemId,
                        GraphDescription(_currentGraph!));
                    result.Validated.Add(contentItem);
                }
                else
                {
                    string message = $"Sync validation failed in {{CurrentGraph}}.{Environment.NewLine}{{validationFailureReason}}.";
                    _logger.LogWarning(message, GraphDescription(_currentGraph!), validationFailureReason);
                    ValidationFailure validationFailure = new ValidationFailure(contentItem, validationFailureReason!, FailureType.Delete);
                    syncFailures.Add(validationFailure);
                    result.ValidationFailures.Add(validationFailure);
                }
            }

            return syncFailures;
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
                "Content items of type {ContentTypeDefinitionName} failed validation ({ValidationFailures}). Attempting to repair them.",
                contentTypeDefinition.Name,
                string.Join(", ", syncValidationFailures.Select(f => f.ContentItem.ToString())));

            foreach (var failure in syncValidationFailures)
            {
                if (failure.Type == FailureType.Merge)
                {
                    await AttemptMergeRepair(contentTypeDefinition, contentItemVersion, result, failure);
                }
                else
                {
                    await AttemptDeleteRepair(contentTypeDefinition, contentItemVersion, result, failure);
                }
            }
        }

        private async Task AttemptMergeRepair(
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion,
            ValidateAndRepairResult result,
            ValidationFailure failure)
        {
            var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
            IGraphReplicaSet graphReplicaSet = _currentGraph!.GetReplicaSetLimitedToThisGraph();

            try
            {
                await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(graphReplicaSet, failure.ContentItem, _contentManager);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Repair of {ContentItem} in {GraphReplicaSet} failed.",
                    failure.ContentItem, graphReplicaSet);
            }

            (bool validated, string? validationFailureReason) =
                await ValidateContentItem(failure.ContentItem, contentTypeDefinition, contentItemVersion);

            if (validated)
            {
                _logger.LogInformation("Repair was successful on {ContentType} {ContentItemId} in {CurrentGraph}.",
                    failure.ContentItem.ContentType,
                    failure.ContentItem.ContentItemId,
                    GraphDescription(_currentGraph!));
                result.Repaired.Add(failure.ContentItem);
            }
            else
            {
                string message = $"Repair was unsuccessful.{Environment.NewLine}{{ValidationFailureReason}}.";
                _logger.LogWarning(message, validationFailureReason);
                result.RepairFailures.Add(new RepairFailure(failure.ContentItem, validationFailureReason!));
            }
        }

        private async Task AttemptDeleteRepair(
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion,
            ValidateAndRepairResult result,
            ValidationFailure failure)
        {
            var deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

            try
            {
                await deleteGraphSyncer.DeleteIfAllowed(failure.ContentItem, contentItemVersion, SyncOperation.Delete);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Repair of deleted {ContentItem} failed.", failure.ContentItem);
            }

            (bool validated, string? validationFailureReason) =
                await ValidateDeletedContentItem(failure.ContentItem, contentTypeDefinition, contentItemVersion);

            if (validated)
            {
                _logger.LogInformation("Repair was successful on deleted {ContentType} {ContentItemId} in {CurrentGraph}.",
                    failure.ContentItem.ContentType,
                    failure.ContentItem.ContentItemId,
                    GraphDescription(_currentGraph!));
                result.Repaired.Add(failure.ContentItem);
            }
            else
            {
                string message = $"Repair was unsuccessful.{Environment.NewLine}{{ValidationFailureReason}}.";
                _logger.LogWarning(message, validationFailureReason);
                result.RepairFailures.Add(new RepairFailure(failure.ContentItem, validationFailureReason!));
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion)
        {
            _logger.LogDebug("Validating {ContentType} {ContentItemId} '{ContentDisplayText}'.",
                contentItem.ContentType, contentItem.ContentItemId, contentItem.DisplayText);

            _syncNameProvider.ContentType = contentItem.ContentType;

            object nodeId = _syncNameProvider.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            //todo: one query to fetch outgoing and incoming
            List<INodeWithOutgoingRelationships?> results = await _currentGraph!.Run(
                new NodeWithOutgoingRelationshipsQuery(
                    await _syncNameProvider.NodeLabels(),
                    _syncNameProvider.IdPropertyName(),
                    nodeId));

            //todo: does this belong in the query?
            INodeWithOutgoingRelationships? nodeWithOutgoingRelationships = results.FirstOrDefault();
            if (nodeWithOutgoingRelationships == null)
                return (false, FailureContext("Node not found querying outgoing relationships.", contentItem));

            List<INodeWithIncomingRelationships?> incomingResults = await _currentGraph!.Run(
                new NodeWithIncomingRelationshipsQuery(
                    await _syncNameProvider.NodeLabels(),
                    _syncNameProvider.IdPropertyName(),
                    nodeId));

            INodeWithIncomingRelationships? nodeWithIncomingRelationships = incomingResults.FirstOrDefault();
            if (nodeWithIncomingRelationships == null)
                return (false, FailureContext("Node not found querying incoming relationships.", contentItem));

            ValidateAndRepairItemSyncContext context = new ValidateAndRepairItemSyncContext(
                contentItem, _contentManager, contentItemVersion, nodeWithOutgoingRelationships,
                nodeWithIncomingRelationships, _syncNameProvider, _graphValidationHelper, this,
                contentTypeDefinition, nodeId, _serviceProvider);

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not?
                if (itemSyncer.CanSync(contentItem))
                {
                    (bool validated, string failureReason) =
                        await itemSyncer.ValidateSyncComponent(context);
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

        private async Task<(bool validated, string failureReason)> ValidateDeletedContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion)
        {
            _logger.LogDebug("Validating deleted {ContentType} {ContentItemId} '{ContentDisplayText}'.",
                contentItem.ContentType, contentItem.ContentItemId, contentItem.DisplayText);

            _syncNameProvider.ContentType = contentItem.ContentType;

            object nodeId = _syncNameProvider.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            //todo: one query to fetch outgoing and incoming
            List<INodeWithOutgoingRelationships?> results = await _currentGraph!.Run(
                new NodeWithOutgoingRelationshipsQuery(
                    await _syncNameProvider.NodeLabels(),
                    _syncNameProvider.IdPropertyName(),
                    nodeId));

            return results.Any()
                ? (false, $"{contentTypeDefinition.DisplayName} {contentItem.ContentItemId} is still present in the graph.")
                : (true, "");
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
}
