using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Events;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    #pragma warning disable S1186
    public class GraphSyncContentDefinitionHandler : IContentDefinitionEventHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentItemsService _contentItemsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphCluster _graphCluster;
        private readonly INotifier _notifier;
        private readonly ILogger<GraphSyncContentDefinitionHandler> _logger;

        public GraphSyncContentDefinitionHandler(
            IContentDefinitionManager contentDefinitionManager,
            IContentItemsService contentItemsService,
            IServiceProvider serviceProvider,
            IGraphCluster graphCluster,
            INotifier notifier,
            ILogger<GraphSyncContentDefinitionHandler> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentItemsService = contentItemsService;
            _serviceProvider = serviceProvider;
            _graphCluster = graphCluster;
            _notifier = notifier;
            _logger = logger;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context)
        {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context)
        {
            //todo:
            // try
            // {
            //     //Delete all nodes by type
            //     await Task.WhenAll(
            //         _deleteGraphSyncer.DeleteNodesByType(GraphReplicaSetNames.Published, context.ContentTypeDefinition.Name),
            //         _deleteGraphSyncer.DeleteNodesByType(GraphReplicaSetNames.Preview, context.ContentTypeDefinition.Name));
            // }
            // catch (Exception)
            // {
            //     _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteContentTypeFromGraphTask),
            //         $"Error: The {typeToDelete} could not be removed because the associated node could not be deleted from the graph. Most likely due to {typeToDelete} having incoming relationships."));
            //     throw;
            // }
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
            //todo: think we need to update following a part removal, in addition to a field removal: add story
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

            try
            {
                #pragma warning disable

                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();
                var affectedContentTypeNames = contentTypeDefinitions
                    .Where(t => t.Parts
                        .Any(p => p.PartDefinition.Name == context.ContentPartName))
                    .Select(t => t.Name);;

                // what's the difference between load and get?

                foreach (string affectedContentTypeName in affectedContentTypeNames)
                {
                    ResyncContentItems(affectedContentTypeName).GetAwaiter().GetResult();
                }

            }
            catch (Exception e)
            {
                string message =
                    $"Graph resync failed after deleting the {context.ContentFieldName} field from {context.ContentPartName} parts.";
                _logger.LogError(e, message);
                _notifier.Add(NotifyType.Error,
                    new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler), message));
                throw;
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

            //todo: old data isn't removed from content item's contents
            // check removed field is removed from the sync (i.e. should do if the the part's field definitions have been updated by this point)

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
