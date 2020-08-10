using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
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

        public GraphResyncer(
            IContentItemsService contentItemsService,
            IServiceProvider serviceProvider,
            IGraphCluster graphCluster)
        {
            _contentItemsService = contentItemsService;
            _serviceProvider = serviceProvider;
            _graphCluster = graphCluster;
        }

        public async Task ResyncContentItems(string contentType)
        {
            var publishedGraphReplicaSet = _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Published);
            var previewGraphReplicaSet = _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Preview);

            var contentItems = await _contentItemsService.GetPublishedOnly(contentType);

            //todo: helper
            foreach (ContentItem contentItem in contentItems)
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                //todo: inject?
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(publishedGraphReplicaSet, contentItem, contentManager);

                mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                //todo: inject?
                contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(previewGraphReplicaSet, contentItem, contentManager);
            }

            contentItems = await _contentItemsService.GetPublishedWithDraftVersion(contentType);

            foreach (ContentItem contentItem in contentItems)
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                //todo: inject?
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(publishedGraphReplicaSet, contentItem, contentManager);
            }

            contentItems = await _contentItemsService.GetDraft(contentType);

            foreach (ContentItem contentItem in contentItems)
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                //todo: inject?
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(previewGraphReplicaSet, contentItem, contentManager);
            }
        }
    }
}
