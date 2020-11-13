using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using ISession = YesSql.ISession;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    public class GraphSyncContentHandler : ContentHandlerBase
    {
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly IDeleteOrchestrator _deleteOrchestrator;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GraphSyncContentHandler> _logger;

        public GraphSyncContentHandler(
            ISyncOrchestrator syncOrchestrator,
            IDeleteOrchestrator deleteOrchestrator,
            ISession session,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GraphSyncContentHandler> logger)
        {
            _syncOrchestrator = syncOrchestrator;
            _deleteOrchestrator = deleteOrchestrator;
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        //todo: add log scopes for these operations

        public override async Task CreatingAsync(CreateContentContext context)
        {
            // the presence of this key indicates that a content item is being restored by the audit trail module
            if (!_httpContextAccessor.HttpContext.Items.ContainsKey("OrchardCore.AuditTrail.Restored"))
                return;

            try
            {
                if (!await _syncOrchestrator.SaveDraft(context.ContentItem))
                {
                    // sad paths have already been notified to the user and logged
                    Cancel(context);
                }
            }
            catch (Exception ex)
            {
                // we log the exception, even though some exceptions will have already been logged,
                // as there might have been an 'unexpected' exception thrown
                _logger.LogError(ex, "Exception saving draft.");
                Cancel(context);
            }
        }

        //todo: there's no DraftSavingAsync (either add it to oc, or raise an issue)
        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            try
            {
                if (!await _syncOrchestrator.SaveDraft(context.ContentItem))
                {
                    // sad paths have already been notified to the user and logged
                    Cancel(context);
                }
            }
            catch (Exception ex)
            {
                // we log the exception, even though some exceptions will have already been logged,
                // as there might have been an 'unexpected' exception thrown
                _logger.LogError(ex, "Exception saving draft.");
                Cancel(context);
            }
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            try
            {
                if (!await _syncOrchestrator.Publish(context.ContentItem))
                {
                    // sad paths have already been notified to the user and logged
                    Cancel(context);
                }
            }
            catch (Exception ex)
            {
                // we log the exception, even though some exceptions will have already been logged,
                // as there might have been an 'unexpected' exception thrown
                _logger.LogError(ex, "Exception publishing.");
                Cancel(context);
            }
        }

        public override async Task UnpublishingAsync(PublishContentContext context)
        {
            try
            {
                if (!await _deleteOrchestrator.Unpublish(context.ContentItem))
                {
                    // sad paths have already been notified to the user and logged
                    Cancel(context);
                }
            }
            catch (Exception ex)
            {
                // we log the exception, even though some exceptions will have already been logged,
                // as there might have been an 'unexpected' exception thrown
                _logger.LogError(ex, "Exception unpublishing.");
                Cancel(context);
            }
        }

        public override async Task CloningAsync(CloneContentContext context)
        {
            try
            {
                if (!await _syncOrchestrator.Clone(context.CloneContentItem))
                {
                    // sad paths have already been notified to the user and logged
                    Cancel(context);
                }
            }
            catch (Exception ex)
            {
                // we log the exception, even though some exceptions will have already been logged,
                // as there might have been an 'unexpected' exception thrown
                _logger.LogError(ex, "Exception cloning.");
                Cancel(context);
            }
        }

        // State          Action     Context:latest      published     no active version left
        // Pub            Delete            0            0                1
        // Pub+Draft      Discard Draft     0            0                0
        // Draft          Delete            0            0                1
        // Pub+Draft      Delete            0            0                1
        public override async Task RemovingAsync(RemoveContentContext context)
        {
            try
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
            catch (Exception ex)
            {
                // we log the exception, even though some exceptions will have already been logged,
                // as there might have been an 'unexpected' exception thrown
                _logger.LogError(ex, "Exception removing (deleting or discarding draft).");
                Cancel(context);
            }
        }

#pragma warning disable S1172

        // some derived contexts don't have a Cancel flag,
        // so we handle them at the base context level by cancelling the session
        private void Cancel(ContentContextBase context)
        {
            _session.Cancel();
        }

#pragma warning restore S1172

        private void Cancel(PublishContentContext context)
        {
            // the oc code checks Cancel in the context, but the item is still published (unpublished?) when you set it
            _session.Cancel();
            context.Cancel = true;
        }
    }
}
