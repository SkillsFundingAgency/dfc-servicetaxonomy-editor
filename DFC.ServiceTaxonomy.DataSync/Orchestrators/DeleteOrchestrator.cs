using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using DFC.ServiceTaxonomy.DataSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.DataSync.Orchestrators
{
    public class DeleteOrchestrator : Orchestrator, IDeleteOrchestrator
    {
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;

        public DeleteOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            IDataSyncNotifier notifier,
            IServiceProvider serviceProvider,
            ILogger<DeleteOrchestrator> logger,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers)
            : base(contentDefinitionManager, notifier, serviceProvider, contentOrchestrationHandlers, logger)
        {
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
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

            await CallContentOrchestrationHandlers(contentItem,
                async (handler, context) => await handler.Unpublished(context));

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

            (IAllowSync publishedAllowSync, IDeleteDataSyncer? publishedDeleteGraphSyncer) = deleteGraphSyncers[0];
            (IAllowSync previewAllowSync, IDeleteDataSyncer? previewDeleteGraphSyncer) = deleteGraphSyncers[1];

            if (publishedAllowSync.Result == AllowSyncResult.NotRequired &&
                previewAllowSync.Result == AllowSyncResult.NotRequired)
            {
                // No graphsyncpart so nothing to do here
                return true;
            }

            if (publishedAllowSync.Result == AllowSyncResult.Blocked || previewAllowSync.Result == AllowSyncResult.Blocked)
            {
                var graphBlockers = new List<(string GraphReplicaSetName, IAllowSync AllowSyncResult)>();
                if (publishedAllowSync.Result == AllowSyncResult.Blocked)
                {
                    graphBlockers.Add((_publishedContentItemVersion.DataSyncReplicaSetName, publishedAllowSync));
                }
                if (previewAllowSync.Result == AllowSyncResult.Blocked)
                {
                    graphBlockers.Add((_previewContentItemVersion.DataSyncReplicaSetName, previewAllowSync));
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

            await CallContentOrchestrationHandlers(contentItem,
                async (handler, context) => await handler.Deleted(context));

            return true;
        }

        private async Task<bool> DeleteFromGraphReplicaSetIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation)
        {
            (IAllowSync allowSync, IDeleteDataSyncer? publishedDeleteGraphSyncer)
                = await GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    syncOperation);

            switch (allowSync.Result)
            {
                case AllowSyncResult.Blocked:
                    await _notifier.AddBlocked(
                        syncOperation,
                        contentItem,
                        new[] {(GraphReplicaSetName: contentItemVersion.DataSyncReplicaSetName, allowSync)});
                    return false;

                case AllowSyncResult.Allowed:
                    await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem, syncOperation);
                    break;
            }

            return true;
        }
    }
}
