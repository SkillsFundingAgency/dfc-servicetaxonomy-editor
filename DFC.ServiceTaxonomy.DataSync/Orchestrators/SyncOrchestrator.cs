using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using DFC.ServiceTaxonomy.DataSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.DataSync.Orchestrators
{
    public class SyncOrchestrator : Orchestrator, ISyncOrchestrator
    {
        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;

        public SyncOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            IDataSyncNotifier notifier,
            IDataSyncCluster dataSyncCluster,
            IServiceProvider serviceProvider,
            ILogger<SyncOrchestrator> logger,
            IPublishedContentItemVersion publishedContentItemVersion,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers)
            : base(contentDefinitionManager, notifier, serviceProvider, contentOrchestrationHandlers, logger)
        {
            _dataSyncCluster = dataSyncCluster;
            _publishedContentItemVersion = publishedContentItemVersion;
        }

        public async Task<bool> Restore(ContentItem contentItem)
        {
            _logger.LogDebug("Restore: Syncing '{ContentItem}' {ContentType} to Preview and deleting from Published.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            //todo: could/should we use WhenAll?

            (IAllowSync previewAllowSync, IMergeDataSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Restore,
                    DataSyncReplicaSetNames.Preview,
                    contentItem, contentManager);

            if (previewAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(
                    SyncOperation.Restore,
                    contentItem,
                    new[] { (DataSyncReplicaSetNames.Preview, previewAllowSync) });
                return false;
            }

            (IAllowSync publishedAllowSync, IDeleteDataSyncer? publishedDeleteGraphSyncer) =
                await GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _publishedContentItemVersion, SyncOperation.Restore);

            if (publishedAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(
                    SyncOperation.Restore,
                    contentItem,
                    new[] { (DataSyncReplicaSetNames.Published, publishedAllowSync) });
                return false;
            }

            if (publishedAllowSync.Result == AllowSyncResult.Allowed
                && previewAllowSync.Result == AllowSyncResult.Allowed)
            {
                await SyncToGraphReplicaSet(SyncOperation.Restore, previewMergeGraphSyncer!, contentItem);
                await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem, SyncOperation.Restore);

                await CallContentOrchestrationHandlers(contentItem,
                    async (handler, context) => await handler.Restored(context));
            }

            return true;
        }

        /// <returns>false if saving draft to preview graph was blocked or failed.</returns>
        public async Task<bool> SaveDraft(ContentItem contentItem)
        {
            _logger.LogDebug("SaveDraft: Syncing '{ContentItem}' {ContentType} to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            return await SyncToGraphReplicaSetIfAllowed(
                SyncOperation.SaveDraft,
                DataSyncReplicaSetNames.Preview,
                contentItem,
                contentManager,
                async (handler, context) => await handler.DraftSaved(context));
        }

        /// <returns>false if publish to either graph was blocked or failed.</returns>
        public async Task<bool> Publish(ContentItem contentItem)
        {
            _logger.LogDebug("Publish: Syncing '{ContentItem}' {ContentType} to Published and Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            // need to leave these calls serial, with published first,
            // so that published can examine the existing preview graph,
            // when it's figuring out what relationships it needs to recreate
            (IAllowSync publishedAllowSync, IMergeDataSyncer? publishedMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Publish,
                    DataSyncReplicaSetNames.Published,
                    contentItem, contentManager);

            if (publishedAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(
                    SyncOperation.Publish,
                    contentItem,
                    new []{(DataSyncReplicaSetNames.Published, publishedAllowSync)});
                return false;
            }

            (IAllowSync previewAllowSync, IMergeDataSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Publish,
                    DataSyncReplicaSetNames.Preview,
                    contentItem, contentManager);

            if (previewAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(
                    SyncOperation.Publish,
                    contentItem,
                    new []{(DataSyncReplicaSetNames.Preview, previewAllowSync)});
                return false;
            }

            if (publishedAllowSync.Result == AllowSyncResult.Allowed
                && previewAllowSync.Result == AllowSyncResult.Allowed)
            {
                // again, not concurrent and published first (for recreating incoming relationships)
                await SyncToGraphReplicaSet(SyncOperation.Publish, publishedMergeGraphSyncer!, contentItem);
                await SyncToGraphReplicaSet(SyncOperation.Publish, previewMergeGraphSyncer!, contentItem);

                await CallContentOrchestrationHandlers(contentItem,
                    async (handler, context) => await handler.Published(context));
            }

            return true;
        }

        /// <returns>false if updating either graph was blocked or failed.</returns>
        public async Task<bool> Update(ContentItem publishedContentItem, ContentItem previewContentItem)
        {
            _logger.LogDebug("Update: Syncing '{PublishedContentItem}' {PublishedContentType} to Published and '{PreviewContentItem}' {PreviewContentType} to Preview.",
                publishedContentItem.ToString(), publishedContentItem.ContentType,
                previewContentItem.ToString(), previewContentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            // need to leave these calls serial, with published first,
            // so that published can examine the existing preview graph,
            // when it's figuring out what relationships it needs to recreate
            (IAllowSync publishedAllowSync, IMergeDataSyncer? publishedMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Update,
                    DataSyncReplicaSetNames.Published,
                    publishedContentItem, contentManager);

            if (publishedAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(SyncOperation.Update, publishedContentItem, new []{(DataSyncReplicaSetNames.Published, publishedAllowSync)});
                return false;
            }

            (IAllowSync previewAllowSync, IMergeDataSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(SyncOperation.Update,
                    DataSyncReplicaSetNames.Preview, previewContentItem, contentManager);

            if (previewAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(SyncOperation.Update, previewContentItem, new []{(DataSyncReplicaSetNames.Preview, previewAllowSync)});
                return false;
            }

            if (publishedAllowSync.Result == AllowSyncResult.Allowed
                && previewAllowSync.Result == AllowSyncResult.Allowed)
            {
                // again, not concurrent and published first (for recreating incoming relationships)
                await SyncToGraphReplicaSet(SyncOperation.Update, publishedMergeGraphSyncer!, publishedContentItem);
                await SyncToGraphReplicaSet(SyncOperation.Update, previewMergeGraphSyncer!, previewContentItem);

                await CallContentOrchestrationHandlers(previewContentItem,
                    async (handler, context) => await handler.DraftSaved(context));

                await CallContentOrchestrationHandlers(publishedContentItem,
                    async (handler, context) => await handler.Published(context));
            }

            return true;
        }

        /// <returns>false if discarding draft was blocked or failed.</returns>
        public async Task<bool> DiscardDraft(ContentItem contentItem)
        {
            _logger.LogDebug("DiscardDraft: Discarding draft '{ContentItem}' {ContentType} by syncing existing Published to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            //todo: check for null
            ContentItem? publishedContentItem =
                await _publishedContentItemVersion.GetContentItem(contentManager, contentItem.ContentItemId);

            return await SyncToGraphReplicaSetIfAllowed(
                SyncOperation.DiscardDraft,
                DataSyncReplicaSetNames.Preview,
                publishedContentItem!,
                contentManager,
                async (handler, context) => await handler.DraftDiscarded(context));
        }

        /// <returns>false if saving clone to preview graph was blocked or failed.</returns>
        public async Task<bool> Clone(ContentItem contentItem)
        {
            _logger.LogDebug("Clone: Syncing mutated cloned '{ContentItem}' {ContentType} to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            ICloneDataSync cloneDataSync = _serviceProvider.GetRequiredService<ICloneDataSync>();

            await cloneDataSync.MutateOnClone(contentItem, contentManager);

            return await SyncToGraphReplicaSetIfAllowed(
                SyncOperation.Clone,
                DataSyncReplicaSetNames.Preview,
                contentItem,
                contentManager,
                async (handler, context) => await handler.Cloned(context));
        }

        //todo: remove equivalent in mergegraphsyncer?
        private async Task<bool> SyncToGraphReplicaSetIfAllowed(
            SyncOperation syncOperation,
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager,
            Func<IContentOrchestrationHandler, IOrchestrationContext, Task> callHandlerWhenAllowed)
        {
            (IAllowSync allowSync, IMergeDataSyncer? mergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(syncOperation, replicaSetName, contentItem, contentManager);

            switch (allowSync.Result)
            {
                case AllowSyncResult.Blocked:
                    await _notifier.AddBlocked(syncOperation, contentItem, new []{(replicaSetName, allowSync)});
                    return false;
                case AllowSyncResult.Allowed:
                    await SyncToGraphReplicaSet(syncOperation, mergeGraphSyncer!, contentItem);
                    await CallContentOrchestrationHandlers(contentItem, callHandlerWhenAllowed);
                    break;
            }
            return true;
        }

        private async Task<(IAllowSync, IMergeDataSyncer?)> GetMergeGraphSyncerIfSyncAllowed(
            SyncOperation syncOperation,
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            try
            {
                IMergeDataSyncer mergeDataSyncer = _serviceProvider.GetRequiredService<IMergeDataSyncer>();

                IAllowSync allowSync = await mergeDataSyncer.SyncAllowed(
                    _dataSyncCluster.GetDataSyncReplicaSet(replicaSetName),
                    contentItem,
                    contentManager);

                return (allowSync, mergeDataSyncer);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                _logger.LogError(exception,
                    "Unable to check if {ContentItemDisplayText} {ContentType} can be synced to {ReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, replicaSetName);

                await _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                    $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be synced to the {replicaSetName} graph, as {nameof(GetMergeGraphSyncerIfSyncAllowed)} threw an exception.",
                    exception: exception);

                throw;
            }
        }

        private async Task SyncToGraphReplicaSet(
            SyncOperation syncOperation,
            IMergeDataSyncer mergeDataSyncer,
            ContentItem contentItem)
        {
            try
            {
                await mergeDataSyncer.SyncToDataSyncReplicaSet();
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                _logger.LogError(exception, "Unable to sync '{ContentItemDisplayText}' {ContentType} to the {DataSyncReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, mergeDataSyncer.DataSyncMergeContext?.DataSyncReplicaSet.Name);
                await _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                    $"Unable to sync '{contentItem.DisplayText}' {contentType} to the {mergeDataSyncer.DataSyncMergeContext?.DataSyncReplicaSet.Name} graph.",
                    exception: exception);
                throw;
            }
        }
    }
}
