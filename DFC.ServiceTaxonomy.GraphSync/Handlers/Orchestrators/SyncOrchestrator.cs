using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
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
        private readonly ILogger<SyncOrchestrator> _logger;

        public SyncOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IGraphCluster graphCluster,
            IPublishedContentItemVersion publishedContentItemVersion,
            IServiceProvider serviceProvider,
            ILogger<SyncOrchestrator> logger)
            : base(contentDefinitionManager, notifier, logger)
        {
            _graphCluster = graphCluster;
            _publishedContentItemVersion = publishedContentItemVersion;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <returns>true if saving draft to preview graph was blocked.</returns>
        public async Task<bool> SaveDraft(ContentItem contentItem)
        {
            _logger.LogDebug("SaveDraft: Syncing '{ContentItem}' {ContentType} to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = GetRequiredService<IContentManager>();

            return await SyncToGraphReplicaSetIfAllowed(
                GraphReplicaSetNames.Preview,
                contentItem,
                contentManager);
        }

        /// <returns>true if publish to either graph was blocked.</returns>
        public async Task<bool> Publish(ContentItem contentItem)
        {
            _logger.LogDebug("Publish: Syncing '{ContentItem}' {ContentType} to Published and Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = GetRequiredService<IContentManager>();

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

            return true;
        }

        /// <returns>true if discarding draft was blocked.</returns>
        public async Task<bool> DiscardDraft(ContentItem contentItem)
        {
            _logger.LogDebug("DiscardDraft: Discarding draft '{ContentItem}' {ContentType} by syncing existing Published to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = GetRequiredService<IContentManager>();

            ContentItem publishedContentItem =
                await _publishedContentItemVersion.GetContentItem(contentManager, contentItem.ContentItemId);

            return await SyncToGraphReplicaSetIfAllowed(
                GraphReplicaSetNames.Preview,
                publishedContentItem,
                contentManager);
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
                /*SyncStatus.NotRequired*/ _ => true
            };
        }

        private async Task<(SyncStatus, IMergeGraphSyncer?)> GetMergeGraphSyncerIfSyncAllowed(
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            try
            {
                IMergeGraphSyncer mergeGraphSyncer = GetRequiredService<IMergeGraphSyncer>();

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

                string message = $"Unable to check if {contentItem.DisplayText} {contentType} can be synced to {replicaSetName} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));

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

                string message = $"Unable to sync '{contentItem.DisplayText}' {contentType} to {mergeGraphSyncer.GraphMergeContext?.GraphReplicaSet.Name} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
                return false;
            }
        }

        // we could use MS Fakes and use the standard extension method instead
        private T GetRequiredService<T>()
        {
            Type type = typeof(T);
            var service =(T)_serviceProvider.GetService(type);
            if (service == null)
                throw new InvalidOperationException($"Couldn't get required service {type.Name}.");
            return service;
        }
    }
}
