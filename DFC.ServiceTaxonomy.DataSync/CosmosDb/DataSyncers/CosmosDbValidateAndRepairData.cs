using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.DataSync.Enums;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;
using DFC.ServiceTaxonomy.DataSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers
{
    public class CosmosDbValidateAndRepairData : IValidateAndRepairData
    {
        private readonly IEnumerable<IContentItemDataSyncer> _itemSyncers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IDataSyncClusterLowLevel _dataSyncClusterLowLevel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IDataSyncValidationHelper _dataSyncValidationHelper;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly IContentItemsService _contentItemsService;
        private readonly ILogger<CosmosDbValidateAndRepairData> _logger;
        private IDataSync? _currentDataSync;
        private static readonly SemaphoreSlim _serialValidation = new SemaphoreSlim(1, 1);

        public CosmosDbValidateAndRepairData(IEnumerable<IContentItemDataSyncer> itemSyncers,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session,
            IServiceProvider serviceProvider,
            ISyncNameProvider syncNameProvider,
            IDataSyncValidationHelper dataSyncValidationHelper,
            IContentItemVersionFactory contentItemVersionFactory,
            IContentItemsService contentItemsService,
            ILogger<CosmosDbValidateAndRepairData> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _serviceProvider = serviceProvider;
            _syncNameProvider = syncNameProvider;
            _dataSyncValidationHelper = dataSyncValidationHelper;
            _contentItemVersionFactory = contentItemVersionFactory;
            _contentItemsService = contentItemsService;
            _logger = logger;
            _currentDataSync = default;

            _dataSyncClusterLowLevel = _serviceProvider.GetRequiredService<IDataSyncClusterLowLevel>();
        }

        public async Task<IValidateAndRepairResults> ValidateData(
            ValidationScope validationScope,
            params string[] dataSyncReplicaSetNames)
        {
            if (!await _serialValidation.WaitAsync(TimeSpan.Zero))
                return ValidationAlreadyInProgressResult.EmptyInstance;

            try
            {
                return await ValidateDataSyncImpl(validationScope, dataSyncReplicaSetNames);
            }
            finally
            {
                _serialValidation.Release();
            }
        }

        private async Task<IValidateAndRepairResults> ValidateDataSyncImpl(
            ValidationScope validationScope,
            params string[] dataSyncReplicaSetNames)
        {
            IEnumerable<ContentTypeDefinition> syncableContentTypeDefinitions = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)));

            DateTime timestamp = DateTime.UtcNow;

            IEnumerable<string> dataSyncReplicaSetNamesToValidate = dataSyncReplicaSetNames.Any()
                ? dataSyncReplicaSetNames
                : _dataSyncClusterLowLevel.DataSyncReplicaSetNames;

            DateTime validateFrom = await GetValidateFromTime(validationScope);
            var results = new ValidateAndRepairResults(validateFrom);

            //todo: we could optimise to only get content items from the oc database once for each replica set,
            // rather than for each instance
            foreach (string dataSyncReplicaSetName in dataSyncReplicaSetNamesToValidate)
            {
                IDataSyncReplicaSetLowLevel dataSyncReplicaSetLowLevel = _dataSyncClusterLowLevel.GetDataSyncReplicaSetLowLevel(dataSyncReplicaSetName);
                IContentItemVersion contentItemVersion = _contentItemVersionFactory.Get(dataSyncReplicaSetName);

                foreach (IDataSync dataSync in dataSyncReplicaSetLowLevel.DataSyncInstances)
                {
                    ValidateAndRepairResult result = results.AddNewValidateAndRepairResult(
                        dataSyncReplicaSetName,
                        dataSync.Instance,
                        dataSync.Endpoint.Name,
                        dataSync.DataSyncName,
                        dataSync.DefaultDataSync);

                    // make current dataSync available for when parts/fields call back into ValidateContentItem
                    // seems a little messy, and will make concurrent validation a pita,
                    // but stops part/field syncers needing low level dataSync access
                    _currentDataSync = dataSync;

                    foreach (ContentTypeDefinition contentTypeDefinition in syncableContentTypeDefinitions)
                    {
                        List<ValidationFailure> syncFailures = await ValidateContentItemsOfContentType(
                            contentItemVersion,
                            contentTypeDefinition,
                            validateFrom,
                            result,
                            validationScope);

                        if (syncFailures.Any())
                        {
                            await AttemptRepair(syncFailures, contentTypeDefinition, contentItemVersion, result);
                        }
                    }
                }
            }

            if (results.AnyRepairFailures)
                return results;

            _logger.LogInformation("Woohoo: data sync passed validation or was successfully repaired at {Time}.",
                timestamp.ToString("O"));

            _session.Save(new AuditSyncLog(timestamp));
            await _session.CurrentTransaction.CommitAsync();

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
            ValidateAndRepairResult result,
            ValidationScope scope)
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
                    _logger.LogInformation("Sync validation passed for {ContentType} {ContentItemId} in {CurrentDataSync}.",
                        contentItem.ContentType,
                        contentItem.ContentItemId,
                        DataSyncDescription(_currentDataSync!));
                    result.Validated.Add(new ValidatedContentItem(contentItem));
                }
                else
                {
                    string message = $"Sync validation failed in {{CurrentDataSync}}.{Environment.NewLine}{{ValidationFailureReason}}.";
                    _logger.LogWarning(message, DataSyncDescription(_currentDataSync!), validationFailureReason);
                    ValidationFailure validationFailure = new ValidationFailure(contentItem, validationFailureReason!);
                    syncFailures.Add(validationFailure);
                    result.ValidationFailures.Add(validationFailure);
                }
            }

            if (scope == ValidationScope.ModifiedSinceLastValidation)
            {
                foreach (ContentItem contentItem in deletedContentTypeContentItems)
                {
                    (bool validated, string? validationFailureReason) =
                        await ValidateDeletedContentItem(contentItem, contentTypeDefinition, contentItemVersion);

                    if (validated)
                    {
                        _logger.LogInformation(
                            "Sync validation passed for deleted {ContentType} {ContentItemId} in {CurrentDataSync}.",
                            contentItem.ContentType,
                            contentItem.ContentItemId,
                            DataSyncDescription(_currentDataSync!));
                        result.Validated.Add(new ValidatedContentItem(contentItem, ValidateType.Delete));
                    }
                    else
                    {
                        string message =
                            $"Sync validation failed in {{CurrentDataSync}}.{Environment.NewLine}{{validationFailureReason}}.";
                        _logger.LogWarning(message, DataSyncDescription(_currentDataSync!), validationFailureReason);
                        ValidationFailure validationFailure = new ValidationFailure(contentItem,
                            validationFailureReason!, ValidateType.Delete);
                        syncFailures.Add(validationFailure);
                        result.ValidationFailures.Add(validationFailure);
                    }
                }
            }

            return syncFailures;
        }

        //todo: ToString in DataSync?
        private string DataSyncDescription(IDataSync dataSync)
        {
            return $"dataSync instance #{dataSync.Instance} in replica set {dataSync.DataSyncReplicaSetLowLevel.Name}";
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
                if (failure.Type == ValidateType.Merge)
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
            var mergeDataSyncer = _serviceProvider.GetRequiredService<IMergeDataSyncer>();
            IDataSyncReplicaSet dataSyncReplicaSet = _currentDataSync!.GetReplicaSetLimitedToThisDataSync();

            try
            {
                await mergeDataSyncer.SyncToDataSyncReplicaSetIfAllowed(dataSyncReplicaSet, failure.ContentItem, _contentManager);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Repair of {ContentItem} in {DataSyncReplicaSet} failed.",
                    failure.ContentItem, dataSyncReplicaSet);
            }

            (bool validated, string? validationFailureReason) =
                await ValidateContentItem(failure.ContentItem, contentTypeDefinition, contentItemVersion);

            if (validated)
            {
                _logger.LogInformation("Repair was successful on {ContentType} {ContentItemId} in {CurrentDataSync}.",
                    failure.ContentItem.ContentType,
                    failure.ContentItem.ContentItemId,
                    DataSyncDescription(_currentDataSync!));
                result.Repaired.Add(new ValidatedContentItem(failure.ContentItem));
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
            var deleteDataSyncer = _serviceProvider.GetRequiredService<IDeleteDataSyncer>();

            try
            {
                await deleteDataSyncer.DeleteIfAllowed(failure.ContentItem, contentItemVersion, SyncOperation.Delete);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Repair of deleted {ContentItem} failed.", failure.ContentItem);
            }

            (bool validated, string? validationFailureReason) =
                await ValidateDeletedContentItem(failure.ContentItem, contentTypeDefinition, contentItemVersion);

            if (validated)
            {
                _logger.LogInformation("Repair was successful on deleted {ContentType} {ContentItemId} in {CurrentDataSync}.",
                    failure.ContentItem.ContentType,
                    failure.ContentItem.ContentItemId,
                    DataSyncDescription(_currentDataSync!));
                result.Repaired.Add(new ValidatedContentItem(failure.ContentItem, ValidateType.Delete));
            }
            else
            {
                string message = $"Repair was unsuccessful.{Environment.NewLine}{{ValidationFailureReason}}.";
                _logger.LogWarning(message, validationFailureReason);
                result.RepairFailures.Add(new RepairFailure(failure.ContentItem, validationFailureReason!, ValidateType.Delete));
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

            object nodeId = _syncNameProvider.GetNodeIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            var nodeLabels = await _syncNameProvider.NodeLabels();

            SubDataSync? nodeWithRelationships = (await _currentDataSync!.Run(
                new CosmosDbSubDataSyncQuery(nodeLabels, _syncNameProvider.IdPropertyName(), nodeId)))
                .FirstOrDefault();

            if (nodeWithRelationships?.SourceNode == null)
                return (false, FailureContext("Node not found querying incoming relationships.", contentItem));

            ValidateAndRepairItemSyncContext context = new ValidateAndRepairItemSyncContext(
                contentItem, _contentManager, contentItemVersion, nodeWithRelationships,
                _syncNameProvider, _dataSyncValidationHelper, this,
                contentTypeDefinition, nodeId, _serviceProvider);

            foreach (IContentItemDataSyncer itemSyncer in _itemSyncers)
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
                int relationshipsInDataSyncCount =
                    nodeWithRelationships.OutgoingRelationships.Count(r => r.Type == relationshipType);

                if (relationshipsInDbCount != relationshipsInDataSyncCount)
                {
                    return (false, FailureContext(
                        $"Expecting {relationshipsInDbCount} relationships of type {relationshipType} in dataSync, but found {relationshipsInDataSyncCount}.",
                        contentItem));
                }
            }

            return (true, string.Empty);
        }

        private async Task<(bool validated, string failureReason)> ValidateDeletedContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion)
        {
            _logger.LogDebug("Validating deleted {ContentType} {ContentItemId} '{ContentDisplayText}'.",
                contentItem.ContentType, contentItem.ContentItemId, contentItem.DisplayText);

            _syncNameProvider.ContentType = contentItem.ContentType;

            object nodeId = _syncNameProvider.GetNodeIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            // this is basically querying to see if the node's there or not - a simpler query might be better
            ISubDataSync? nodeWithRelationships = (await _currentDataSync!.Run(
                    new CosmosDbSubDataSyncQuery(
                        await _syncNameProvider.NodeLabels(),
                        _syncNameProvider.IdPropertyName(),
                        nodeId,
                        CosmosDbSubDataSyncQuery.RelationshipFilterNone, 0)))
                .FirstOrDefault();

            return nodeWithRelationships?.SourceNode != null
                ? (false, $"{contentTypeDefinition.DisplayName} {contentItem.ContentItemId} is still present in the dataSync.")
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
