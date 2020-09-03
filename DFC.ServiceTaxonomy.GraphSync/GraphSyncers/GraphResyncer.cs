using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class GraphResyncer : IGraphResyncer
    {
        private readonly IContentItemsService _contentItemsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphCluster _graphCluster;
        private readonly IContentManager _contentManager;

        public GraphResyncer(
            IContentItemsService contentItemsService,
            IServiceProvider serviceProvider,
            IGraphCluster graphCluster,
            IContentManager contentManager)
        {
            _contentItemsService = contentItemsService;
            _serviceProvider = serviceProvider;
            _graphCluster = graphCluster;
            _contentManager = contentManager;
        }

        public async Task ResyncContentItems(string contentType)
        {
            var publishedGraphReplicaSet = _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Published);
            var previewGraphReplicaSet = _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Preview);

            var contentItems = await _contentItemsService.GetPublishedOnly(contentType);
            await Sync(contentItems, publishedGraphReplicaSet);
            await Sync(contentItems, previewGraphReplicaSet);

            contentItems = await _contentItemsService.GetPublishedWithDraftVersion(contentType);
            await Sync(contentItems, publishedGraphReplicaSet);

            contentItems = await _contentItemsService.GetDraft(contentType);
            await Sync(contentItems, previewGraphReplicaSet);
        }

        private async Task Sync(IEnumerable<ContentItem> contentItems, IGraphReplicaSet graphReplicaSet)
        {
            foreach (ContentItem contentItem in contentItems)
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(graphReplicaSet, contentItem, _contentManager);
            }
        }
    }
}
