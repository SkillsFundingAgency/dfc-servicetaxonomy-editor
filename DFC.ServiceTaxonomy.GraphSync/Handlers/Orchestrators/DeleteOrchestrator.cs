using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class DeleteOrchestrator : Orchestrator, IDeleteOrchestrator
    {
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;

        public DeleteOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IServiceProvider serviceProvider,
            ILogger<DeleteOrchestrator> logger,
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            ISyncNameProvider syncNameProvider,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            INeutralEventContentItemVersion neutralEventContentItemVersion)
            : base(contentDefinitionManager, notifier, logger, eventGridConfiguration, eventGridContentClient, syncNameProvider, publishedContentItemVersion, previewContentItemVersion, neutralEventContentItemVersion)
        {
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
        }

        /// <returns>true if unpublish to publish graph was blocked.</returns>
        public async Task<bool> Unpublish(ContentItem contentItem)
        {
            _logger.LogDebug("Unpublish: Removing '{ContentItem}' {ContentType} from Published.",
                contentItem.ToString(), contentItem.ContentType);

            // no need to touch the draft graph, there should always be a valid version in there
            // (either a separate draft version, or the published version)

            if (!await DeleteFromGraphReplicaSetIfAllowed(
                contentItem,
                _publishedContentItemVersion,
                DeleteOperation.Unpublish))
            {
                return false;
            }

            //if unpublish was successful publish events
            await PublishContentEvent(contentItem, ContentEventType.Unpublished);
            await PublishContentEvent(contentItem, ContentEventType.Draft);

            return true;
        }

        /// <returns>true if deleting from either graph was blocked.</returns>
        public async Task<bool> Delete(ContentItem contentItem)
        {
            // we could be more selective deciding which replica set to delete from,
            // but the context doesn't seem to be there, and our delete is idempotent

            _logger.LogDebug("Delete: Removing '{ContentItem}' {ContentType} from Published and/or Preview.",
                contentItem.ToString(), contentItem.ContentType);

            var deleteGraphSyncers = await Task.WhenAll(
                GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _publishedContentItemVersion, DeleteOperation.Delete),
                GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem, _previewContentItemVersion, DeleteOperation.Delete));

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

            if (!await DeleteFromGraphReplicaSet(previewDeleteGraphSyncer!, contentItem))
            {
                return false;
            }

            if (!await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem))
            {
                return false;
            }

            //if everything succeeded, publish event and return true
            await PublishContentEvent(contentItem, ContentEventType.Deleted);

            return true;
        }

        private async Task<bool> DeleteFromGraphReplicaSetIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation)
        {
            (SyncStatus syncStatus, IDeleteGraphSyncer? publishedDeleteGraphSyncer)
                = await GetDeleteGraphSyncerIfDeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    deleteOperation);

            return syncStatus switch
            {
                SyncStatus.Blocked => false,
                SyncStatus.Allowed => await DeleteFromGraphReplicaSet(publishedDeleteGraphSyncer!, contentItem),
                /*SyncStatus.NotRequired*/ _ => true
            };
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

                //todo: use AddFailureNotifier? pass variables as params to log
                string message = $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be {(deleteOperation == DeleteOperation.Delete?"deleted":"unpublished")} from {contentItemVersion.GraphReplicaSetName} graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));

                return (SyncStatus.Blocked, null);
            }
        }

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

                AddFailureNotifier(contentItem);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Couldn't delete from graph replica set.");
                AddFailureNotifier(contentItem);
            }
            return false;
        }
    }
}
