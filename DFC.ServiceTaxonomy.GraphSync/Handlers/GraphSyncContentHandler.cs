using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Models;
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
        private readonly IGraphSyncHelper _graphSyncHelper;

        public GraphSyncContentHandler(
            IGraphCluster graphCluster,
            IServiceProvider serviceProvider,
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            ILogger<GraphSyncContentHandler> logger,
            IGraphSyncHelper graphSyncHelper)
        {
            _graphCluster = graphCluster;
            _serviceProvider = serviceProvider;
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _logger = logger;
            _graphSyncHelper = graphSyncHelper;
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            await SyncToGraphReplicaSetIfAllowed(GraphReplicaSetNames.Preview, context.ContentItem, contentManager);
        }

        private IMergeGraphSyncer? _previewMergeGraphSyncer;
        private IMergeGraphSyncer? _publishedMergeGraphSyncer;

        private IDeleteGraphSyncer? _previewDeleteGraphSyncer;
        private IDeleteGraphSyncer? _publishedDeleteGraphSyncer;

        public override async Task PublishingAsync(PublishContentContext context)
        {
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            _previewMergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
            _publishedMergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

            var syncAllowed = await Task.WhenAll(
                AllowSyncToGraphReplicaSet(_previewMergeGraphSyncer, GraphReplicaSetNames.Preview, context.ContentItem, contentManager),
                AllowSyncToGraphReplicaSet(_publishedMergeGraphSyncer, GraphReplicaSetNames.Published, context.ContentItem, contentManager));

            //todo: can be improved in c#9
            // sad paths have already been notified to the user and logged
            context.Cancel = syncAllowed[0].AllowSync == SyncStatus.Blocked
                             || syncAllowed[0].AllowSync == SyncStatus.CheckFailed
                             || syncAllowed[1].AllowSync == SyncStatus.Blocked
                             || syncAllowed[1].AllowSync == SyncStatus.CheckFailed;

            if (context.Cancel)
            {
                AddBlockedNotifier(GraphReplicaSetNames.Preview, syncAllowed[0], context.ContentItem);
                AddBlockedNotifier(GraphReplicaSetNames.Published, syncAllowed[1], context.ContentItem);
            }
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            await Task.WhenAll(
                SyncToGraphReplicaSet(_previewMergeGraphSyncer!, GraphReplicaSetNames.Preview, context.ContentItem),
                SyncToGraphReplicaSet(_publishedMergeGraphSyncer!, GraphReplicaSetNames.Published, context.ContentItem));
        }

        #pragma warning disable
        private void AddBlockedNotifier(string graphReplicaSetName, IAllowSyncResult allowSyncResult, ContentItem contentItem)
        {
            if (allowSyncResult.AllowSync != SyncStatus.Blocked)
                return;

            //todo: delegate string creation?

            //todo: we get each blocker twice because of 2 graphs in replica (think) - either list blockers by graph or distinct them
            //todo: need details of the content item with incoming relationships
            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                $"Publishing to the {graphReplicaSetName} graphs has been cancelled. These items relate: {string.Join(", ", allowSyncResult.SyncBlockers.Select(b => $"{b.Title} {b.ContentType}"))}"));
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            return Task.CompletedTask;
        }

        public override async Task UnpublishingAsync(PublishContentContext context)
        {
            _publishedDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

            var deleteAllowed = await AllowDeleteFromReplicaSet(
                _publishedDeleteGraphSyncer,
                context.ContentItem,
                _publishedContentItemVersion,
                DeleteOperation.Unpublish);

            //todo: unpublish is passed a PublishContentContext, so cancel is probably ignored
            context.Cancel = deleteAllowed.AllowSync == SyncStatus.Blocked
                             || deleteAllowed.AllowSync == SyncStatus.CheckFailed;

            if (context.Cancel)
            {
                AddBlockedNotifier(GraphReplicaSetNames.Published, deleteAllowed, context.ContentItem);
            }
        }

        //todo: switch to ing handlers
        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            //todo: we need to decide how to handle this. do we leave a placeholder node (in the pub graph)
            // with a property to say item has no published version
            // or check incoming relationships and cancel unpublish?
            await DeleteFromGraphReplicaSet(_publishedDeleteGraphSyncer, context.ContentItem);

            // no need to touch the draft graph, there should always be a valid version in there
            // (either a separate draft version, or the published version)
        }

        private async Task<IAllowSyncResult> AllowDeleteFromReplicaSet(
            IDeleteGraphSyncer deleteGraphSyncer,
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation)
        {
            try
            {
                return await deleteGraphSyncer.DeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    deleteOperation);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                //todo: change deleted to unpublished where appropriate?
                string message = $"Unable to check if {contentItem.DisplayText} {contentType} can be deleted from {contentItemVersion.GraphReplicaSetName} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
            }

            return AllowSyncResult.CheckFailed;
        }

        public override async Task ClonedAsync(CloneContentContext context)
        {
            if (context.CloneContentItem.Content[nameof(GraphSyncPart)] != null)
            {
                _graphSyncHelper.ContentType = context.CloneContentItem.ContentType;
                context.CloneContentItem.Content[nameof(GraphSyncPart)][nameof(GraphSyncPart.Text)] = await _graphSyncHelper.GenerateIdPropertyValue();
            }
        }

        public override async Task RemovingAsync(RemoveContentContext context)
        {
            _publishedDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();
            _previewDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

            var deleteAllowed = await Task.WhenAll(
                AllowDeleteFromReplicaSet(_previewDeleteGraphSyncer, context.ContentItem, _previewContentItemVersion, DeleteOperation.Delete),
                AllowDeleteFromReplicaSet(_publishedDeleteGraphSyncer, context.ContentItem, _publishedContentItemVersion, DeleteOperation.Delete));

            //todo: helpers for repeated code
            bool cancelPreview = deleteAllowed[0].AllowSync == SyncStatus.Blocked
                                 || deleteAllowed[0].AllowSync == SyncStatus.CheckFailed;

            bool cancelPublished = deleteAllowed[1].AllowSync == SyncStatus.Blocked
                                   || deleteAllowed[1].AllowSync == SyncStatus.CheckFailed;

            //todo: removing doesn't have it's own context with a cancel, so will have to cancel the session
            //context.Cancel = cancelPreview || cancelPublished;

            //todo: elsewhere, only add notifiers for graphs that were blocked
            if (cancelPreview)
            {
                AddBlockedNotifier(GraphReplicaSetNames.Preview, deleteAllowed[0], context.ContentItem);
            }

            if (cancelPublished)
            {
                //todo: no magic number
                //todo: get name from deleteGraphSyncer?
                AddBlockedNotifier(GraphReplicaSetNames.Published, deleteAllowed[1], context.ContentItem);
            }
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
                    DeleteFromGraphReplicaSet(_previewDeleteGraphSyncer, context.ContentItem),
                    DeleteFromGraphReplicaSet(_publishedDeleteGraphSyncer, context.ContentItem));
                return;
            }

            // discard draft event
            //todo: if not allowed cancel delete, return bool?
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            ContentItem publishedContentItem = await _publishedContentItemVersion.GetContentItem(contentManager, context.ContentItem.ContentItemId);

            await SyncToGraphReplicaSetIfAllowed(GraphReplicaSetNames.Preview, publishedContentItem, contentManager);
        }

        private async Task DeleteFromGraphReplicaSet(IDeleteGraphSyncer deleteGraphSyncer, ContentItem contentItem)
        {
            try
            {
                await deleteGraphSyncer.Delete();
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message != "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    //todo: use Removing event instead
                    _session.Cancel();
                    AddFailureNotifier(contentItem);
                    throw;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Couldn't delete from graph replica set.");
                _session.Cancel();
                AddFailureNotifier(contentItem);
                throw;
            }
        }

        private async Task SyncToGraphReplicaSetIfAllowed(
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

            //todo: use MergeGraphSyncer syncifallowed?

            IAllowSyncResult allowSyncResult = await AllowSyncToGraphReplicaSet(mergeGraphSyncer, replicaSetName, contentItem, contentManager);

            switch (allowSyncResult.AllowSync)
            {
                case SyncStatus.Allowed:
                    await SyncToGraphReplicaSet(mergeGraphSyncer, replicaSetName, contentItem);
                    break;
                case SyncStatus.Blocked:
                    string contentType = GetContentTypeDisplayName(contentItem);

                    //todo:
                    string message = $"Syncing {contentItem.DisplayText} {contentType} to the {replicaSetName} graph has been blocked: todo reason";
                    _logger.LogInformation(message);
                    _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
                    break;
            }
        }

        private async Task<IAllowSyncResult> AllowSyncToGraphReplicaSet(
            IMergeGraphSyncer mergeGraphSyncer,
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            try
            {
                return await mergeGraphSyncer.SyncAllowed(
                    _graphCluster.GetGraphReplicaSet(replicaSetName),
                    contentItem,
                    contentManager);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                string message = $"Unable to check if {contentItem.DisplayText} {contentType} can be synced to {replicaSetName} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
            }

            return AllowSyncResult.CheckFailed;
        }

        private async Task SyncToGraphReplicaSet(
            IMergeGraphSyncer mergeGraphSyncer,
            string replicaSetName,
            ContentItem contentItem)
        {
            try
            {
                await mergeGraphSyncer.SyncToGraphReplicaSet();
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                string message = $"Unable to sync '{contentItem.DisplayText}' {contentType} to {replicaSetName} graph.";
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
