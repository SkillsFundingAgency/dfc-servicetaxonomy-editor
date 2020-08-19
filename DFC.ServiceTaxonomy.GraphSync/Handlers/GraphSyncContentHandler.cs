using System;
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
#pragma warning disable S1172

    // inject sync and delete classes

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

        //todo: add log scopes for these operations

        //todo: there's no DraftSavingAsync (either add it to oc, or raise an issue)
        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (!await SaveDraft(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
            }
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            if (!await Publish(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
            }
        }

        /// <returns>true if saving draft to preview graph was blocked.</returns>
        private async Task<bool> SaveDraft(ContentItem contentItem)
        {
            _logger.LogDebug("SaveDraft: Syncing '{ContentItem}' {ContentType} to Preview.",
                contentItem.ToString(), contentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            return await SyncToGraphReplicaSetIfAllowed(
                GraphReplicaSetNames.Preview,
                contentItem,
                contentManager) == SyncStatus.Blocked;
        }

        /// <returns>true if publish to either graph was blocked.</returns>
        private async Task<bool> Publish(ContentItem contentItem)//PublishContentContext context)
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

            return true;
        }

        public override async Task UnpublishingAsync(PublishContentContext context)
        {
            if (!await Unpublish(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
            }
        }

        /// <returns>true if unpublish to publish graph was blocked.</returns>
        public async Task<bool> Unpublish(ContentItem contentItem)
        {
            _logger.LogDebug("Unpublish: Removing '{ContentItem}' {ContentType} from Published.",
                contentItem.ToString(), contentItem.ContentType);

            // no need to touch the draft graph, there should always be a valid version in there
            // (either a separate draft version, or the published version)

            return await DeleteFromGraphReplicaSetIfAllowed(
                contentItem,
                _publishedContentItemVersion,
                DeleteOperation.Unpublish) == SyncStatus.Blocked;
        }

        private void Cancel(PublishContentContext context)
        {
            // the oc code checks Cancel in the context, but the item is still published (unpublished?) when you set it
            _session.Cancel();
            context.Cancel = true;
        }

        private void Cancel(SaveDraftContentContext context)
        {
            // there's no cancel on the SaveDraftContentContext context, so we have to cancel the session
            //todo: either add it to oc, or raise an issue

            _session.Cancel();
        }

        private void Cancel(RemoveContentContext context)
        {
            // removing doesn't have it's own context with a cancel, so we have to cancel the session
            //todo: either add them to oc, or raise an issue

            _session.Cancel();
        }

        //todo: do in cloning
        public override async Task ClonedAsync(CloneContentContext context)
        {
            if (context.CloneContentItem.Content[nameof(GraphSyncPart)] != null)
            {
                _graphSyncHelper.ContentType = context.CloneContentItem.ContentType;
                context.CloneContentItem.Content[nameof(GraphSyncPart)][nameof(GraphSyncPart.Text)] = await _graphSyncHelper.GenerateIdPropertyValue();
            }
        }

        // State          Action     Context:latest      published     no active version left
        // Pub            Delete            0            0                1
        // Pub+Draft      Discard Draft     0            0                0
        // Draft          Delete            0            0                1
        // Pub+Draft      Delete            0            0                1
        public override async Task RemovingAsync(RemoveContentContext context)
        {
            if (context.NoActiveVersionLeft)
            {
                if (!await Delete(context))
                {
                    Cancel(context);
                }
                return;
            }

            if (!await DiscardDraft(context))
            {
                Cancel(context);
            }
        }

        /// <returns>true if deleting from either graph was blocked.</returns>
        private async Task<bool> Delete(RemoveContentContext context)
        {
            // we could be more selective deciding which replica set to delete from,
            // but the context doesn't seem to be there, and our delete is idempotent

            _logger.LogDebug("Delete: Removing '{ContentItem}' {ContentType} from Published and/or Preview.",
                context.ContentItem.ToString(), context.ContentItem.ContentType);

            var deleteGraphSyncers = await Task.WhenAll(
                GetDeleteGraphSyncerIfDeleteAllowed(
                    context.ContentItem, _publishedContentItemVersion, DeleteOperation.Delete),
                GetDeleteGraphSyncerIfDeleteAllowed(
                    context.ContentItem, _previewContentItemVersion, DeleteOperation.Delete));

            (SyncStatus publishedSyncStatus, IDeleteGraphSyncer? publishedDeleteGraphSyncer) = deleteGraphSyncers[0];
            (SyncStatus previewSyncStatus, IDeleteGraphSyncer? previewDeleteGraphSyncer) = deleteGraphSyncers[1];

            if (publishedSyncStatus == SyncStatus.Blocked || previewSyncStatus == SyncStatus.Blocked)
            {
                return false;
            }

            // the preview graph contains a superset of the published graph,
            // so we try deleting from the preview graph first, and only move onto published
            // if the preview sync worked
            //todo: add any failure checks into allow check

            if (!await DeleteFromGraphReplicaSet(previewDeleteGraphSyncer!, context.ContentItem))
            {
                return false;
            }

            return await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, context.ContentItem);
        }

        /// <returns>true if discarding draft was blocked.</returns>
        private async Task<bool> DiscardDraft(RemoveContentContext context)
        {
            _logger.LogDebug("DiscardDraft: Discarding draft '{ContentItem}' {ContentType} by syncing existing Published to Preview.",
                context.ContentItem.ToString(), context.ContentItem.ContentType);

            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            ContentItem publishedContentItem =
                await _publishedContentItemVersion.GetContentItem(contentManager, context.ContentItem.ContentItemId);

            return await SyncToGraphReplicaSetIfAllowed(
                       GraphReplicaSetNames.Preview,
                       publishedContentItem,
                       contentManager) == SyncStatus.Blocked;
        }

        private async Task<SyncStatus> DeleteFromGraphReplicaSetIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation)
        {
            (SyncStatus syncStatus, IDeleteGraphSyncer? publishedDeleteGraphSyncer)
                = await GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    deleteOperation);

            if (syncStatus == SyncStatus.Allowed
                && !await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem))
            {
                return SyncStatus.Blocked;
            }

            return syncStatus;
        }

        private async Task<(SyncStatus, IDeleteGraphSyncer?)> GetDeleteGraphSyncerIfDeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation)
        {
            try
            {
                IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

                IAllowSyncResult allowSyncResult = await deleteGraphSyncer.DeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    deleteOperation);

                if (allowSyncResult.AllowSync == SyncStatus.Blocked)
                {
                    AddBlockedNotifier(
                        deleteOperation == DeleteOperation.Delete ? "Deleting from" : "Unpublishing from",
                        contentItemVersion.GraphReplicaSetName, allowSyncResult, contentItem);
                }

                return (allowSyncResult.AllowSync, deleteGraphSyncer);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                string message = $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be {(deleteOperation == DeleteOperation.Delete?"deleted":"unpublished")} from {contentItemVersion.GraphReplicaSetName} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));

                return (SyncStatus.Blocked, null);
            }
        }

        //todo: want cancel at higher level??
        private async Task<bool> DeleteFromGraphReplicaSet(IDeleteGraphSyncer deleteGraphSyncer, ContentItem contentItem)
        {
            try
            {
                await deleteGraphSyncer.Delete();
                return true;
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message == "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    return true;
                }

                //_session.Cancel();
                AddFailureNotifier(contentItem);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Couldn't delete from graph replica set.");
                //_session.Cancel();
                AddFailureNotifier(contentItem);
                //throw;
            }
            return false;
        }

        private async Task<SyncStatus> SyncToGraphReplicaSetIfAllowed(
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            //todo: new operationdescription?
            (SyncStatus syncStatus, IMergeGraphSyncer? mergeGraphSyncer) =
                await GetMergeGraphSyncerIfSyncAllowed(replicaSetName, contentItem, contentManager);

            if (syncStatus == SyncStatus.Allowed)
            {
                await SyncToGraphReplicaSet(mergeGraphSyncer!, contentItem);
            }

            return syncStatus;
        }

        private async Task<(SyncStatus, IMergeGraphSyncer?)> GetMergeGraphSyncerIfSyncAllowed(
            string replicaSetName,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            try
            {
                IMergeGraphSyncer? mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

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
                string contentType = GetContentTypeDisplayName(contentItem);

                string message = $"Unable to sync '{contentItem.DisplayText}' {contentType} to {mergeGraphSyncer.GraphMergeContext?.GraphReplicaSet.Name} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
                return false;
            }
        }

        private void AddBlockedNotifier(
            string operationDescription,
            string graphReplicaSetName,
            IAllowSyncResult allowSyncResult,
            ContentItem contentItem)
        {
            //string contentType = GetContentTypeDisplayName(contentItem);

            string message =
                $"{operationDescription} the {graphReplicaSetName} graphs has been cancelled. These items relate: {allowSyncResult}.";

            _logger.LogWarning(message);

            //todo: need details of the content item with incoming relationships
            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
        }

        private void AddFailureNotifier(ContentItem contentItem)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                $"The '{contentItem.DisplayText}' {contentType} could not be removed because the associated node could not be deleted from the graph."));
        }

        private string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }
    }

#pragma warning restore S1172
}
