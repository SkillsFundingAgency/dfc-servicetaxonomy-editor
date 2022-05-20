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
                new[] { Constants.ContentEvent_Published, Constants.ContentEvent_Unpublished}.Any(ev => ev.Equals(contentEvent.Name, StringComparison.CurrentCultureIgnoreCase)) &&
                contentEvent.AuditTrailEventItem.ContentItem.Has<UnpublishLaterPart>())
            {
                var unpublishLaterPart = contentEvent.AuditTrailEventItem.ContentItem.As<UnpublishLaterPart>();
                if (unpublishLaterPart == null)
                {
                    return Task.CompletedTask;
                }

                // Saved UnpublishLater Event & Cancelled UnpublishLater Event
                if (contentEvent.Name.Equals(Constants.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Cancelled UnpublishLater Event
                    if (_httpContextAccessor.HttpContext.Request.Form[Constants.ContentEvent_Published] == Constants.Submit_Cancel_Unpublish_Later)
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
            savedContentEvent.Name = "Scheduled to be unpublished";
            //var version = unpublishLaterPart.ContentItem.ContentItemVersionId;
            //var contentType = unpublishLaterPart.ContentItem.ContentType;
            //var contentName = unpublishLaterPart.ContentItem.Content.Name;
            //var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            //savedContentEvent.AuditTrailEventItem..Name = $"{version} of the {contentType} {contentName} was scheduled to be unpublished at {scheduledUnpublishUtc}.";
        }

        private static void ProcessCancelUnpublishLaterEvent(AuditTrailCreateContext<AuditTrailContentEvent> cancelUnpublishLaterEvent, UnpublishLaterPart unpublishLaterPart)
        {
            cancelUnpublishLaterEvent.Name = "Cancelled for upublishing";
            //var version = unpublishLaterPart.ContentItem.ContentItemVersionId;
            //var contentType = unpublishLaterPart.ContentItem.ContentType;
            //var contentName = unpublishLaterPart.ContentItem.Content.Name;
            //var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            //cancelUnpublishLaterEvent.Name = $"{version} of the {contentType} {contentName} cancelled an unpublish previously scheduled at {scheduledUnpublishUtc}.";
        }

        private static void ProcessUnpublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, UnpublishLaterPart unpublishLaterPart)
        {
            publishedContentEvent.Name = "Unpublished as per schedule";
            //var version = unpublishLaterPart.ContentItem.ContentItemVersionId;
            //var contentType = unpublishLaterPart.ContentItem.ContentType;
            //var contentName = unpublishLaterPart.ContentItem.Content.Name;
            //var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            //publishedContentEvent.Name = $"{version} of the {contentType} {contentName} was unpublished as per schedule at {scheduledUnpublishUtc}.";
        }
    }
}
