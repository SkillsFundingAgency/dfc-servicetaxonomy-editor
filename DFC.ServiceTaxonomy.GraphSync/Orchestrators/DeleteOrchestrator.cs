using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators
{
    public class DeleteOrchestrator : Orchestrator, IDeleteOrchestrator
    {
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        public DeleteOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncNotifier notifier,
            IServiceProvider serviceProvider,
            ILogger<DeleteOrchestrator> logger,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers)
            : base(contentDefinitionManager, notifier, logger)
        {
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
            _contentOrchestrationHandlers = contentOrchestrationHandlers;
        }

        /// <returns>false if unpublish to publish graph was blocked.</returns>
        public async Task<bool> Unpublish(ContentItem contentItem)
        {
            _logger.LogDebug("Unpublish: Removing '{ContentItem}' {ContentType} from Published.",
                contentItem.ToString(), contentItem.ContentType);

            // no need to touch the draft graph, there should always be a valid version in there
            // (either a separate draft version, or the published version)

            if (!await DeleteFromGraphReplicaSetIfAllowed(
                contentItem,
                _publishedContentItemVersion,
                SyncOperation.Unpublish))
            {
                return false;
            }

            //if unpublish was successful publish events
            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Unpublished(contentItem);
            }

            return true;
        }

        /// <returns>false if deleting from either graph was blocked.</returns>
        public async Task<bool> Delete(ContentItem contentItem)
        {
            // we could be more selective deciding which replica set to delete from,
            // but the context doesn't seem to be there, and our delete is idempotent

            _logger.LogDebug("Delete: Removing '{ContentItem}' {ContentType} from Published and/or Preview.",
                contentItem.ToString(), contentItem.ContentType);

            var deleteGraphSyncers = await Task.WhenAll(
                GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _publishedContentItemVersion, SyncOperation.Delete),
                GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _previewContentItemVersion, SyncOperation.Delete));

            (IAllowSyncResult publishedAllowSyncResult, IDeleteGraphSyncer? publishedDeleteGraphSyncer) = deleteGraphSyncers[0];
            (IAllowSyncResult previewAllowSyncResult, IDeleteGraphSyncer? previewDeleteGraphSyncer) = deleteGraphSyncers[1];


            // publishedAllowSyncResult = AllowSyncResult.TestBlocked;
            // previewAllowSyncResult = AllowSyncResult.TestBlocked;



            if (publishedAllowSyncResult.AllowSync == SyncStatus.Blocked || previewAllowSyncResult.AllowSync == SyncStatus.Blocked)
            {
                var graphBlockers = new List<(string GraphReplicaSetName, IAllowSyncResult AllowSyncResult)>();
                if (publishedAllowSyncResult.AllowSync == SyncStatus.Blocked)
                {
                    graphBlockers.Add((_publishedContentItemVersion.GraphReplicaSetName, publishedAllowSyncResult));
                }
                if (previewAllowSyncResult.AllowSync == SyncStatus.Blocked)
                {
                    graphBlockers.Add((_previewContentItemVersion.GraphReplicaSetName, previewAllowSyncResult));
                }

                await _notifier.AddBlocked(SyncOperation.Delete, contentItem, graphBlockers);
                return false;
            }

            // the preview graph contains a superset of the published graph,
            // so we try deleting from the preview graph first, and only move onto published
            // if the preview sync worked
            //todo: add any failure checks into allow check

            await DeleteFromGraphReplicaSet(previewDeleteGraphSyncer!, contentItem, SyncOperation.Delete);
            await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem, SyncOperation.Delete);

            // if everything succeeded, publish event and return true
            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Deleted(contentItem);
            }

            return true;
        }

        private async Task<bool> DeleteFromGraphReplicaSetIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation)
        {
            (IAllowSyncResult allowSyncResult, IDeleteGraphSyncer? publishedDeleteGraphSyncer)
                = await GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    syncOperation);

            switch (allowSyncResult.AllowSync)
            {
                case SyncStatus.Blocked:
                    await _notifier.AddBlocked(
                        syncOperation,
                        contentItem,
                        new[] {(contentItemVersion.GraphReplicaSetName, allowSyncResult)});
                    return false;

                case SyncStatus.Allowed:
                    await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem, syncOperation);
                    break;
            }

            return true;
        }

        private async Task<(IAllowSyncResult, IDeleteGraphSyncer?)> GetDeleteGraphSyncerIfDeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation)
        {
            try
            {
                IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

                IAllowSyncResult allowSyncResult = await deleteGraphSyncer.DeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    syncOperation);

                return (allowSyncResult, deleteGraphSyncer);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                //todo: will get logged twice, but want to keep the param version
                _logger.LogError(exception, "Unable to check if the '{ContentItem}' {ContentType} can be {DeleteOperation} from the {GraphReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, syncOperation.ToString("PrP", null).ToLower(), contentItemVersion.GraphReplicaSetName);

                _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                    $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be {syncOperation.ToString("PrP", null).ToLower()} from the {contentItemVersion.GraphReplicaSetName} graph.",
                    exception: exception);

                throw;
            }
        }

        private async Task DeleteFromGraphReplicaSet(IDeleteGraphSyncer deleteGraphSyncer,
            ContentItem contentItem,
            SyncOperation syncOperation)
        {
            try
            {
                await deleteGraphSyncer.Delete();
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message == "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    return;
                }

                AddFailureNotifier(deleteGraphSyncer, contentItem, ex, syncOperation);
                throw;
            }
            catch(Exception ex)
            {
                AddFailureNotifier(deleteGraphSyncer, contentItem, ex, syncOperation);
                throw;
            }
        }

        private void AddFailureNotifier(IDeleteGraphSyncer deleteGraphSyncer,
            ContentItem contentItem,
            Exception exception,
            SyncOperation syncOperation)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            string operation = syncOperation.ToString("PrP", null);

            _logger.LogError(exception, "{Operation} the '{ContentItem}' {ContentType} has been cancelled because the {GraphReplicaSetName} graph couldn't be updated.",
                operation, contentItem.DisplayText, contentType, deleteGraphSyncer.GraphReplicaSetName);

            _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                $"{operation} the '{contentItem.DisplayText}' {contentType} has been cancelled because the {deleteGraphSyncer.GraphReplicaSetName} graph couldn't be updated.",
                exception: exception);
        }
    }
}
