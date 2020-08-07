using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Events;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    #pragma warning disable S1186
    public class GraphSyncContentDefinitionHandler : IContentDefinitionEventHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentItemsService _contentItemsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphCluster _graphCluster;

        public GraphSyncContentDefinitionHandler(
            IContentDefinitionManager contentDefinitionManager,
            IContentItemsService contentItemsService,
            IServiceProvider serviceProvider,
            IGraphCluster graphCluster)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentItemsService = contentItemsService;
            _serviceProvider = serviceProvider;
            _graphCluster = graphCluster;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context)
        {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context)
        {
        }

        public void ContentTypeImporting(ContentTypeImportingContext context)
        {
        }

        public void ContentTypeImported(ContentTypeImportedContext context)
        {
        }

        public void ContentPartCreated(ContentPartCreatedContext context)
        {
        }

        public void ContentPartRemoved(ContentPartRemovedContext context)
        {
        }

        public void ContentPartAttached(ContentPartAttachedContext context)
        {
        }

        public void ContentPartDetached(ContentPartDetachedContext context)
        {
        }

        public void ContentPartImporting(ContentPartImportingContext context)
        {
        }

        public void ContentPartImported(ContentPartImportedContext context)
        {
        }

        public void ContentFieldAttached(ContentFieldAttachedContext context)
        {
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context)
        {
            //todo: looks like old code assumed field was deleted from eponymous part (check)
            // so wouln't work with custom parts (as used in job profiles)

//todo: need to get types that use the part
            #pragma warning disable
//            var partDefinition = _contentDefinitionManager.GetPartDefinition(context.ContentPartName);

            IEnumerable<ContentTypeDefinition> contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();
            var affectedContentTypeDefinitions = contentTypeDefinitions
                .Where(t => t.Parts
//                    .Any(p => p.PartDefinition.Name == partDefinition.Name))
                    .Any(p => p.PartDefinition.Name == context.ContentPartName))
                .ToArray();

            //todo: do in one
            var affectedContentTypeNames = affectedContentTypeDefinitions.Select(t => t.Name);

            // what's the difference between load and get?

            foreach (string affectedContentTypeName in affectedContentTypeNames)
            {
                ResyncContentItems(affectedContentTypeName).GetAwaiter().GetResult();
            }
        }
        #pragma warning restore S1186

        //todo: where does this belong?
        public async Task ResyncContentItems(string contentType)
        {
            //inject Preview|publishedContentItemversion?
            var publishedGraphReplicaSet = _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Published);
            var previewGraphReplicaSet = _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Preview);

            var contentItems = await _contentItemsService.GetPublishedOnly(contentType);

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
