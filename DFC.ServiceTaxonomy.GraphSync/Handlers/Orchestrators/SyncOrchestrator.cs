using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class SyncOrchestrator : Orchestrator, ISyncOrchestrator
    {
        private readonly IGraphCluster _graphCluster;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        public SyncOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
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
            (SyncStatus publishedSyncStatus, IMergeGraphSyncer? publishedMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    GraphReplicaSetNames.Published, contentItem, contentManager);

            if (publishedSyncStatus == SyncStatus.Blocked)
            {
                return false;
            }

            (SyncStatus previewSyncStatus, IMergeGraphSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    GraphReplicaSetNames.Preview, contentItem, contentManager);

            if (previewSyncStatus == SyncStatus.Blocked)
            {
                return false;
            }

            // again, not concurrent and published first (for recreating incoming relationships)

            if ((publishedSyncStatus == SyncStatus.Allowed && !await SyncToGraphReplicaSet(publishedMergeGraphSyncer!, contentItem))
                || (previewSyncStatus == SyncStatus.Allowed && !await SyncToGraphReplicaSet(previewMergeGraphSyncer!, contentItem)))
            {
                return false;
            }

            //todo: move these into 'ed' where available?
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
            (SyncStatus publishedSyncStatus, IMergeGraphSyncer? publishedMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    GraphReplicaSetNames.Published, publishedContentItem, contentManager);

            if (publishedSyncStatus == SyncStatus.Blocked)
            {
                return false;
            }

            (SyncStatus previewSyncStatus, IMergeGraphSyncer? previewMergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(
                    GraphReplicaSetNames.Preview, previewContentItem, contentManager);

            if (previewSyncStatus == SyncStatus.Blocked)
            {
                return false;
            }

            // again, not concurrent and published first (for recreating incoming relationships)

            if ((publishedSyncStatus == SyncStatus.Allowed && !await SyncToGraphReplicaSet(publishedMergeGraphSyncer!, publishedContentItem))
                || (previewSyncStatus == SyncStatus.Allowed && !await SyncToGraphReplicaSet(previewMergeGraphSyncer!, previewContentItem)))
            {
                return false;
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
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            //todo: new operationdescription?
            (SyncStatus syncStatus, IMergeGraphSyncer? mergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(replicaSetName, contentItem, contentManager);

            return syncStatus switch
            {
                SyncStatus.Blocked => false,
                SyncStatus.Allowed => await SyncToGraphReplicaSet(mergeGraphSyncer!, contentItem),
                /*SyncStatus.NotRequired*/
                _ => true
            };
        }

        private async Task<(SyncStatus, IMergeGraphSyncer?)> GetMergeGraphSyncerIfSyncAllowed(
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            try
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                IAllowSyncResult allowSyncResult = await mergeGraphSyncer.SyncAllowed(
                    _graphCluster.GetGraphReplicaSet(replicaSetName),
                    contentItem,
                    contentManager);

                if (allowSyncResult.AllowSync == SyncStatus.Blocked)
                {
                    AddBlockedNotifier("Syncing to", replicaSetName, allowSyncResult, contentItem);
                }

                return (allowSyncResult.AllowSync, mergeGraphSyncer);
            }
            catch (Exception exception)
            {
                //todo: use notifier helper
                string contentType = GetContentTypeDisplayName(contentItem);

                _logger.LogError(exception,
                    "Unable to check if {ContentItemDisplayText} {ContentType} can be synced to {ReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, replicaSetName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                    $"Unable to check if {contentItem.DisplayText} {contentType} can be synced to {replicaSetName} graph."));

                return (SyncStatus.Blocked, null);
            }
        }

        private async Task<bool> SyncToGraphReplicaSet(IMergeGraphSyncer mergeGraphSyncer, ContentItem contentItem)
        {
            try
            {
                await mergeGraphSyncer.SyncToGraphReplicaSet();
                return true;
            }
            catch (Exception exception)
            {
                //todo: use notifier helper

                string contentType = GetContentTypeDisplayName(contentItem);

                _logger.LogError(exception, "Unable to sync '{ContentItemDisplayText}' {ContentType} to {GraphReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, mergeGraphSyncer.GraphMergeContext?.GraphReplicaSet.Name);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                    $"Unable to sync '{contentItem.DisplayText}' {contentType} to {mergeGraphSyncer.GraphMergeContext?.GraphReplicaSet.Name} graph."));
                return false;
            }
        }
    }
}
