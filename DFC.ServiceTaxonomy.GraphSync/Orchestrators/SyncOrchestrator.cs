using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators
{
    public class SyncOrchestrator : Orchestrator, ISyncOrchestrator
    {
        private readonly IGraphCluster _graphCluster;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        public SyncOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncNotifier notifier,
            IGraphCluster graphCluster,
            IServiceProvider serviceProvider,
            ILogger<SyncOrchestrator> logger,
            IPublishedContentItemVersion publishedContentItemVersion,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers)
            : base(contentDefinitionManager, notifier, logger)
        {
            _graphCluster = graphCluster;
            _publishedContentItemVersion = publishedContentItemVersion;
            _serviceProvider = serviceProvider;
            _contentOrchestrationHandlers = contentOrchestrationHandlers;
        }

        /// <returns>false if saving draft to preview graph was blocked or failed.</returns>
        public async Task<bool> SaveDraft(ContentItem contentItem)
        {
            _logger.LogDebug("SaveDraft: Syncing '{ContentItem}' {ContentType} to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            if (!await SyncToGraphReplicaSetIfAllowed(
                SyncOperation.SaveDraft,
                GraphReplicaSetNames.Preview,
                contentItem,
                contentManager))
            {
                return false;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.DraftSaved(contentItem);
            }

            return true;
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
            (IAllowSync publishedAllowSync, IMergeGraphSyncer? publishedMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Publish,
                    GraphReplicaSetNames.Published,
                    contentItem, contentManager);

            if (publishedAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(
                    SyncOperation.Publish,
                    contentItem,
                    new []{(GraphReplicaSetNames.Published, publishedAllowSync)});
                return false;
            }

            (IAllowSync previewAllowSync, IMergeGraphSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Publish,
                    GraphReplicaSetNames.Preview,
                    contentItem, contentManager);

            if (previewAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(
                    SyncOperation.Publish,
                    contentItem,
                    new []{(GraphReplicaSetNames.Preview, previewAllowSync)});
                return false;
            }

            // again, not concurrent and published first (for recreating incoming relationships)

            if (publishedAllowSync.Result == AllowSyncResult.Allowed)
            {
                await SyncToGraphReplicaSet(SyncOperation.Publish, publishedMergeGraphSyncer!, contentItem);
            }

            if (previewAllowSync.Result == AllowSyncResult.Allowed)
            {
                await SyncToGraphReplicaSet(SyncOperation.Publish, previewMergeGraphSyncer!, contentItem);
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Published(contentItem);
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
            (IAllowSync publishedAllowSync, IMergeGraphSyncer? publishedMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    SyncOperation.Update,
                    GraphReplicaSetNames.Published,
                    publishedContentItem, contentManager);

            if (publishedAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(SyncOperation.Update, publishedContentItem, new []{(GraphReplicaSetNames.Published, publishedAllowSync)});
                return false;
            }

            (IAllowSync previewAllowSync, IMergeGraphSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(SyncOperation.Update,
                    GraphReplicaSetNames.Preview, previewContentItem, contentManager);

            if (previewAllowSync.Result == AllowSyncResult.Blocked)
            {
                await _notifier.AddBlocked(SyncOperation.Update, previewContentItem, new []{(GraphReplicaSetNames.Preview, previewAllowSync)});
                return false;
            }

            // again, not concurrent and published first (for recreating incoming relationships)

            if (publishedAllowSync.Result == AllowSyncResult.Allowed)
            {
                await SyncToGraphReplicaSet(SyncOperation.Update, publishedMergeGraphSyncer!, publishedContentItem);
            }

            if (previewAllowSync.Result == AllowSyncResult.Allowed)
            {
                await SyncToGraphReplicaSet(SyncOperation.Update, previewMergeGraphSyncer!, previewContentItem);
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.DraftSaved(previewContentItem);
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Published(publishedContentItem);
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

            if (!await SyncToGraphReplicaSetIfAllowed(
                SyncOperation.DiscardDraft,
                GraphReplicaSetNames.Preview,
                publishedContentItem!,
                contentManager))
            {
                return false;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.DraftDiscarded(contentItem);
            }

            return true;
        }

        /// <returns>false if saving clone to preview graph was blocked or failed.</returns>
        public async Task<bool> Clone(ContentItem contentItem)
        {
            _logger.LogDebug("Clone: Syncing mutated cloned '{ContentItem}' {ContentType} to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            ICloneGraphSync cloneGraphSync = _serviceProvider.GetRequiredService<ICloneGraphSync>();

            await cloneGraphSync.MutateOnClone(contentItem, contentManager);

            if (!await SyncToGraphReplicaSetIfAllowed(
                SyncOperation.Clone,
                GraphReplicaSetNames.Preview,
                contentItem,
                contentManager))
            {
                return false;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Cloned(contentItem);
            }

            return true;
        }

        //todo: remove equivalent in mergegraphsyncer?
        private async Task<bool> SyncToGraphReplicaSetIfAllowed(
            SyncOperation syncOperation,
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            (IAllowSync allowSync, IMergeGraphSyncer? mergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(syncOperation, replicaSetName, contentItem, contentManager);

            switch (allowSync.Result)
            {
                case AllowSyncResult.Blocked:
                    await _notifier.AddBlocked(syncOperation, contentItem, new []{(replicaSetName, allowSync)});
                    return false;
                case AllowSyncResult.Allowed:
                    await SyncToGraphReplicaSet(syncOperation, mergeGraphSyncer!, contentItem);
                    break;
            }
            return true;
        }

        #pragma warning disable S1172
        private async Task<(IAllowSync, IMergeGraphSyncer?)> GetMergeGraphSyncerIfSyncAllowed(
            SyncOperation syncOperation,
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            try
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                IAllowSync allowSync = await mergeGraphSyncer.SyncAllowed(
                    _graphCluster.GetGraphReplicaSet(replicaSetName),
                    contentItem,
                    contentManager);

                return (allowSync, mergeGraphSyncer);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                _logger.LogError(exception,
                    "Unable to check if {ContentItemDisplayText} {ContentType} can be synced to {ReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, replicaSetName);

                _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                    $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be synced to the {replicaSetName} graph, as {nameof(GetMergeGraphSyncerIfSyncAllowed)} threw an exception.",
                    exception: exception);

                throw;
            }
        }

        private async Task SyncToGraphReplicaSet(
            SyncOperation syncOperation,
            IMergeGraphSyncer mergeGraphSyncer,
            ContentItem contentItem)
        {
            try
            {
                await mergeGraphSyncer.SyncToGraphReplicaSet();
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                _logger.LogError(exception, "Unable to sync '{ContentItemDisplayText}' {ContentType} to the {GraphReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, mergeGraphSyncer.GraphMergeContext?.GraphReplicaSet.Name);
                _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                    $"Unable to sync '{contentItem.DisplayText}' {contentType} to the {mergeGraphSyncer.GraphMergeContext?.GraphReplicaSet.Name} graph.",
                    exception: exception);
                throw;
            }
        }
    }
}
