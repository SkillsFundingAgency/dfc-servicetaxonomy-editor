using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class DeleteOrchestrator : Orchestrator, IDeleteOrchestrator
    {
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeleteOrchestrator> _logger;

        public DeleteOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IServiceProvider serviceProvider,
            ILogger<DeleteOrchestrator> logger)
            : base(contentDefinitionManager, notifier, logger)
        {
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <returns>true if unpublish to publish graph was blocked.</returns>
        public async Task<bool> Unpublish(ContentItem contentItem)
        {
            _logger.LogDebug("Unpublish: Removing '{ContentItem}' {ContentType} from Published.",
                contentItem.ToString(), contentItem.ContentType);

            // no need to touch the draft graph, there should always be a valid version in there
            // (either a separate draft version, or the published version)

            return await DeleteFromGraphReplicaSetIfAllowed(
                contentItem,
                _publishedContentItemVersion,
                DeleteOperation.Unpublish) == SyncStatus.Blocked;
        }

        /// <returns>true if deleting from either graph was blocked.</returns>
        public async Task<bool> Delete(ContentItem contentItem)
        {
            // we could be more selective deciding which replica set to delete from,
            // but the context doesn't seem to be there, and our delete is idempotent

            _logger.LogDebug("Delete: Removing '{ContentItem}' {ContentType} from Published and/or Preview.",
                contentItem.ToString(), contentItem.ContentType);

            var deleteGraphSyncers = await Task.WhenAll(
                GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _publishedContentItemVersion, DeleteOperation.Delete),
                GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _previewContentItemVersion, DeleteOperation.Delete));

            (SyncStatus publishedSyncStatus, IDeleteGraphSyncer? publishedDeleteGraphSyncer) = deleteGraphSyncers[0];
            (SyncStatus previewSyncStatus, IDeleteGraphSyncer? previewDeleteGraphSyncer) = deleteGraphSyncers[1];

            if (publishedSyncStatus == SyncStatus.Blocked || previewSyncStatus == SyncStatus.Blocked)
            {
                return false;
            }

            // the preview graph contains a superset of the published graph,
            // so we try deleting from the preview graph first, and only move onto published
            // if the preview sync worked
            //todo: add any failure checks into allow check

            if (!await DeleteFromGraphReplicaSet(previewDeleteGraphSyncer!, contentItem))
            {
                return false;
            }

            return await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem);
        }

        private async Task<SyncStatus> DeleteFromGraphReplicaSetIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation)
        {
            (SyncStatus syncStatus, IDeleteGraphSyncer? publishedDeleteGraphSyncer)
                = await GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    deleteOperation);

            if (syncStatus == SyncStatus.Allowed
                && !await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem))
            {
                return SyncStatus.Blocked;
            }

            return syncStatus;
        }

        private async Task<(SyncStatus, IDeleteGraphSyncer?)> GetDeleteGraphSyncerIfDeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation)
        {
            try
            {
                IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

                IAllowSyncResult allowSyncResult = await deleteGraphSyncer.DeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    deleteOperation);

                if (allowSyncResult.AllowSync == SyncStatus.Blocked)
                {
                    AddBlockedNotifier(
                        deleteOperation == DeleteOperation.Delete ? "Deleting from" : "Unpublishing from",
                        contentItemVersion.GraphReplicaSetName, allowSyncResult, contentItem);
                }

                return (allowSyncResult.AllowSync, deleteGraphSyncer);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                //todo: use AddFailureNotifier?
                string message = $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be {(deleteOperation == DeleteOperation.Delete?"deleted":"unpublished")} from {contentItemVersion.GraphReplicaSetName} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));

                return (SyncStatus.Blocked, null);
            }
        }

        private async Task<bool> DeleteFromGraphReplicaSet(IDeleteGraphSyncer deleteGraphSyncer, ContentItem contentItem)
        {
            try
            {
                await deleteGraphSyncer.Delete();
                return true;
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message == "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    return true;
                }

                //_session.Cancel();
                AddFailureNotifier(contentItem);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Couldn't delete from graph replica set.");
                //_session.Cancel();
                AddFailureNotifier(contentItem);
                //throw;
            }
            return false;
        }
    }
}
