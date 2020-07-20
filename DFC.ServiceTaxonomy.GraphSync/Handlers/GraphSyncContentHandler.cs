using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    //todo: add eventgrid content handler too
    public class GraphSyncContentHandler : ContentHandlerBase
    {
        private readonly IGraphCluster _graphCluster;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INotifier _notifier;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly ILogger<GraphSyncContentHandler> _logger;

        public GraphSyncContentHandler(
            IGraphCluster graphCluster,
            IServiceProvider serviceProvider,
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            ILogger<GraphSyncContentHandler> logger)
        {
            _graphCluster = graphCluster;
            _serviceProvider = serviceProvider;
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _logger = logger;
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            await SyncToGraphReplicaSet(GraphReplicaSetNames.Preview, context.ContentItem, contentManager);
        }

        //todo: context contains cancel! (might have to use publishing though)
        public override async Task PublishedAsync(PublishContentContext context)
        {
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            await Task.WhenAll(
                SyncToGraphReplicaSet(GraphReplicaSetNames.Preview, context.ContentItem, contentManager),
                SyncToGraphReplicaSet(GraphReplicaSetNames.Published, context.ContentItem, contentManager));
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            //todo: we need to decide how to handle this. do we leave a placeholder node (in the pub graph)
            // with a property to say item has no published version
            // or check incoming relationships and cancel unpublish?
            await DeleteFromGraphReplicaSet(context.ContentItem, _publishedContentItemVersion);

            // no need to touch the draft graph, there should always be a valid version in there
            // (either a separate draft version, or the published version)
        }

        // State          Action     Context:latest      published     no active version left
        // Pub            Delete            0            0                1
        // Pub+Draft      Discard Draft     0            0                0
        // Draft          Delete            0            0                1
        // Pub+Draft      Delete            0            0                1
        public override async Task RemovedAsync(RemoveContentContext context)
        {
            // we could be more selective deciding which replica set to delete from,
            // but the context doesn't seem to be there, and our delete is idempotent

            if (context.NoActiveVersionLeft)
            {
                await Task.WhenAll(
                    DeleteFromGraphReplicaSet(context.ContentItem, _previewContentItemVersion),
                    DeleteFromGraphReplicaSet(context.ContentItem, _publishedContentItemVersion));
                return;
            }

            // discard draft event
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            ContentItem publishedContentItem = await _publishedContentItemVersion.GetContentItem(contentManager, context.ContentItem.ContentItemId);

            await SyncToGraphReplicaSet(GraphReplicaSetNames.Preview, publishedContentItem, contentManager);
        }

        private async Task DeleteFromGraphReplicaSet(ContentItem contentItem, IContentItemVersion contentItemVersion)
        {
            try
            {
                IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

                await deleteGraphSyncer.Delete(contentItem, contentItemVersion);
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message != "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    _session.Cancel();
                    AddFailureNotifier(contentItem);
                    throw;
                }
            }
            catch
            {
                _session.Cancel();
                AddFailureNotifier(contentItem);
                throw;
            }
        }

        private async Task SyncToGraphReplicaSet(string replicaSetName, ContentItem contentItem, IContentManager contentManager)
        {
            try
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                await mergeGraphSyncer.SyncToGraphReplicaSet(_graphCluster.GetGraphReplicaSet(replicaSetName), contentItem, contentManager);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                string message = $"Unable to sync {contentItem.DisplayText} {contentType} to {replicaSetName} graph(s).";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
            }
        }

        private void AddFailureNotifier(ContentItem contentItem)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                $"The {contentItem.DisplayText} {contentType} could not be removed because the associated node could not be deleted from the graph."));
        }

        private string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }
    }
}
