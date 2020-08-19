using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using OrchardCore.ContentManagement.Handlers;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
#pragma warning disable S1172

    // inject sync and delete classes

    public class GraphSyncContentHandler : ContentHandlerBase
    {
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly IDeleteOrchestrator _deleteOrchestrator;
        private readonly ISession _session;
        private readonly IGraphSyncHelper _graphSyncHelper;

        public GraphSyncContentHandler(
            ISyncOrchestrator syncOrchestrator,
            IDeleteOrchestrator deleteOrchestrator,
            ISession session,
            IGraphSyncHelper graphSyncHelper)
        {
            _syncOrchestrator = syncOrchestrator;
            _deleteOrchestrator = deleteOrchestrator;
            _session = session;
            _graphSyncHelper = graphSyncHelper;
        }

        //todo: add log scopes for these operations

        //todo: there's no DraftSavingAsync (either add it to oc, or raise an issue)
        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (!await _syncOrchestrator.SaveDraft(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
            }
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            if (!await _syncOrchestrator.Publish(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
            }
        }

        public override async Task UnpublishingAsync(PublishContentContext context)
        {
            if (!await _deleteOrchestrator.Unpublish(context.ContentItem))
            {
                // sad paths have already been notified to the user and logged
                Cancel(context);
            }
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
                if (!await _deleteOrchestrator.Delete(context.ContentItem))
                {
                    Cancel(context);
                }
                return;
            }

            if (!await _syncOrchestrator.DiscardDraft(context.ContentItem))
            {
                Cancel(context);
            }
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
    }

#pragma warning restore S1172
}
