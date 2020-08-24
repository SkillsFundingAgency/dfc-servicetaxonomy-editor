using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using OrchardCore.ContentManagement.Handlers;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    public class GraphSyncContentHandler : ContentHandlerBase
    {
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly IDeleteOrchestrator _deleteOrchestrator;
        private readonly ICloneOrchestrator _cloneOrchestrator;
        private readonly ISession _session;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        public GraphSyncContentHandler(
            ISyncOrchestrator syncOrchestrator,
            IDeleteOrchestrator deleteOrchestrator,
            ICloneOrchestrator cloneOrchestrator,
            ISession session,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers)
        {
            _syncOrchestrator = syncOrchestrator;
            _deleteOrchestrator = deleteOrchestrator;
            _cloneOrchestrator = cloneOrchestrator;
            _session = session;
            _contentOrchestrationHandlers = contentOrchestrationHandlers;
        }

        //todo: add log scopes for these operations

        //todo: there's no DraftSavingAsync (either add it to oc, or raise an issue)
        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (!await _syncOrchestrator.SaveDraft(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
                return;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.DraftSaved(context.ContentItem);
            }
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            if (!await _syncOrchestrator.Publish(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
                return;
            }

            //todo: move these into 'ed' where available?
            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Published(context.ContentItem);
            }
        }

        public override async Task UnpublishingAsync(PublishContentContext context)
        {
            if (!await _deleteOrchestrator.Unpublish(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
                return;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Unpublished(context.ContentItem);
            }
        }

        public override async Task ClonedAsync(CloneContentContext context)
        {
            if (!await _cloneOrchestrator.Clone(context.CloneContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
                return;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.Cloned(context.CloneContentItem);
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
                if (!await _deleteOrchestrator.Delete(context.ContentItem))
                {
                    Cancel(context);
                    return;
                }

                foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
                {
                    await contentOrchestrationHandler.Deleted(context.ContentItem);
                }
                return;
            }

            if (!await _syncOrchestrator.DiscardDraft(context.ContentItem))
            {
                Cancel(context);
                return;
            }

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await contentOrchestrationHandler.DraftDiscarded(context.ContentItem);
            }
        }

        private void Cancel(PublishContentContext context)
        {
            // the oc code checks Cancel in the context, but the item is still published (unpublished?) when you set it
            _session.Cancel();
            context.Cancel = true;
        }

#pragma warning disable S1172

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

        private void Cancel(CloneContentContext context)
        {
            _session.Cancel();
        }

#pragma warning restore S1172
    }
}
