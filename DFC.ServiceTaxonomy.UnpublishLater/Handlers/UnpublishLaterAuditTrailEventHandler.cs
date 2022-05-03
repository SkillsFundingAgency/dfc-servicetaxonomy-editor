using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.UnpublishLater.Handlers
{
    public class UnpublishLaterAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UnpublishLaterAuditTrailEventHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            // Override event name if:
            // 1. it is audit trail content event
            // 2. it is a Save or Unpublished events only 
            // 3. there is a UnpublishLaterPart add as part of the content type
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent &&
                new[] { Constants.ContentEvent_Saved, Constants.ContentEvent_Unpublished}.Any(ev => ev.Equals(contentEvent.Name, StringComparison.CurrentCultureIgnoreCase)) &&
                contentEvent.AuditTrailEventItem.ContentItem.Has<UnpublishLaterPart>())
            {
                var unpublishLaterPart = contentEvent.AuditTrailEventItem.ContentItem.As<UnpublishLaterPart>();
                if (unpublishLaterPart == null)
                {
                    return Task.CompletedTask;
                }

                // Saved UnpublishLater Event & Cancelled UnpublishLater Event
                if (contentEvent.Name.Equals(Constants.ContentEvent_Saved, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Cancelled UnpublishLater Event
                    if (_httpContextAccessor.HttpContext.Request.Form["submit.Save"] == "submit.CancelUnpublishLater")
                    {
                        ProcessCancelUnpublishLaterEvent(contentEvent, unpublishLaterPart);
                    }

                    // Saved UnpublishLater Event
                    if (unpublishLaterPart.ScheduledUnpublishUtc.HasValue)
                    {
                        ProcessSavedEvent(contentEvent, unpublishLaterPart);
                    }
                } // UnpublishLater Event
                else if (contentEvent.Name.Equals(Constants.ContentEvent_Unpublished, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (unpublishLaterPart.ScheduledUnpublishUtc.HasValue)
                    {
                        ProcessUnpublishedEvent(contentEvent, unpublishLaterPart);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private static void ProcessSavedEvent(AuditTrailCreateContext<AuditTrailContentEvent> savedContentEvent, UnpublishLaterPart unpublishLaterPart)
        {
            savedContentEvent.Name = GetSavedEventName(unpublishLaterPart);
        }

        private static void ProcessCancelUnpublishLaterEvent(AuditTrailCreateContext<AuditTrailContentEvent> cancelUnpublishLaterEvent, UnpublishLaterPart unpublishLaterPart)
        {
            cancelUnpublishLaterEvent.Name = GetCancelUnpublishLaterEventName(unpublishLaterPart);
        }

        private static void ProcessUnpublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, UnpublishLaterPart unpublishLaterPart)
        {
            publishedContentEvent.Name = GetUnpublishedEventName(unpublishLaterPart);
        }

        private static string GetSavedEventName(UnpublishLaterPart unpublishLaterPart)
        {
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            return $"Item is to be unpublished at ({scheduledUnpublishUtc})";
        }

        private static string GetCancelUnpublishLaterEventName(UnpublishLaterPart unpublishLaterPart)
        {
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            return $"CancelUnpublishLaterEvent at ({scheduledUnpublishUtc})";
        }
        private static string GetUnpublishedEventName(UnpublishLaterPart unpublishLaterPart)
        {
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            return $"Item was sheduled unpublished at ({scheduledUnpublishUtc})";
        }
    }
}
