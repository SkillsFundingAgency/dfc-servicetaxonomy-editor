using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.ContentManagement;
using OrchardCore.PublishLater.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers
{
    public class CustomAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        private readonly IHttpContextAccessor  _httpContextAccessor;

        public CustomAuditTrailEventHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> removedEvent && removedEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Removed, StringComparison.OrdinalIgnoreCase))
            {
                // No way to divine whether a Remove event is a delete or discarded draft from the content item so need to look a the URL.
                var isDiscard = _httpContextAccessor.HttpContext.Request.Path.Value.Contains(Constants.AuditTrail.UrlPart_DiscardDraft, StringComparison.CurrentCultureIgnoreCase);
                if(isDiscard)
                {
                    removedEvent.Name = Constants.AuditTrail.ContentEventName_DiscardDraft;
                }
            }

            // Override event name if:
            // 1. it is audit trail content event
            // 2. it is a Save or Published events only 
            // 3. there is a PublishLaterPart add as part of the content type
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent && new[] { Constants.AuditTrail.ContentEvent_Saved, Constants.AuditTrail.ContentEvent_Published }.Any(ev => ev.Equals(contentEvent.Name, StringComparison.CurrentCultureIgnoreCase)) && contentEvent.AuditTrailEventItem.ContentItem.Has<PublishLaterPart>())
            {
                var publishLaterPart = contentEvent.AuditTrailEventItem.ContentItem.As<PublishLaterPart>();
                if (publishLaterPart == null)
                {
                    return Task.CompletedTask;
                }

                // Saved PublishLater Event & Cancelled PublishLater Event
                if (contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Saved, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Cancelled PublishLater Event
                    if (_httpContextAccessor.HttpContext.Request.Form["submit.Save"] == "submit.CancelPublishLater")
                    {
                        ProcessCancelPublishLaterEvent(contentEvent, publishLaterPart);
                    }

                    // Saved PublishLater Event
                    if (publishLaterPart.ScheduledPublishUtc.HasValue)
                    {
                        ProcessSavedEvent(contentEvent, publishLaterPart);
                    }
                } // PublishLater Event
                else if (contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (publishLaterPart.ScheduledPublishUtc.HasValue)
                    {
                        ProcessPublishedEvent(contentEvent, publishLaterPart);
                    }
                }
                else {
                    // for sonar
                }
            }

            return Task.CompletedTask;
        }

        private static void ProcessSavedEvent(AuditTrailCreateContext<AuditTrailContentEvent> savedContentEvent, PublishLaterPart publishLaterPart)
        {
            savedContentEvent.Name = GetSavedEventName(publishLaterPart);
        }

        private static void ProcessCancelPublishLaterEvent(AuditTrailCreateContext<AuditTrailContentEvent> cancelPublishLaterEvent, PublishLaterPart publishLaterPart)
        {
            cancelPublishLaterEvent.Name = GetCancelPublishLaterEventName(publishLaterPart);
        }

        private static void ProcessPublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, PublishLaterPart publishLaterPart)
        {
            publishedContentEvent.Name = GetPublishedEventName(publishLaterPart);
        }

        private static string GetSavedEventName(PublishLaterPart publishLaterPart)
        {
            var scheduledPublishUtc = publishLaterPart.ScheduledPublishUtc.HasValue ? publishLaterPart.ScheduledPublishUtc.Value.ToString() : "no value";
            return $"Item is to be published at ({scheduledPublishUtc})";
        }

        private static string GetCancelPublishLaterEventName(PublishLaterPart publishLaterPart)
        {
            var scheduledPublishUtc = publishLaterPart.ScheduledPublishUtc.HasValue ? publishLaterPart.ScheduledPublishUtc.Value.ToString() : "no value";
            return $"Item will no longer be published at ({scheduledPublishUtc})";
        }
        private static string GetPublishedEventName(PublishLaterPart publishLaterPart)
        {
            var scheduledPublishUtc = publishLaterPart.ScheduledPublishUtc.HasValue ? publishLaterPart.ScheduledPublishUtc.Value.ToString() : "no value";
            return $"Item was sheduled published at ({scheduledPublishUtc})";
        }
    }
}
