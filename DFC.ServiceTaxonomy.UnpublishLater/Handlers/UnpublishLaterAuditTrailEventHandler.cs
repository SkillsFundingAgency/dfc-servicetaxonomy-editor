using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.UnpublishLater.Handlers
{
    public class UnpublishLaterAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            // Override event name if:
            // 1. it is audit trail content event
            // 2. it is a Save, Publish or Unpublished events only - (to add cancel) 
            // 3. there is a UnpublishLaterPart add as part of the content type
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent &&
                new[] { Constants.ContentEvent_Saved, Constants.ContentEvent_Published, Constants.ContentEvent_Unpublished }.Any(ev => ev.Equals(contentEvent.Name, StringComparison.CurrentCultureIgnoreCase)) &&
                contentEvent.AuditTrailEventItem.ContentItem.Has<UnpublishLaterPart>())
            {
                var unpublishLaterPart = contentEvent.AuditTrailEventItem.ContentItem.As<UnpublishLaterPart>();
                if (unpublishLaterPart == null)
                {
                    return Task.CompletedTask;
                }

                //if (contentEvent.Name.Equals(Constants.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    if (unpublishLaterPart.ScheduledUnpublishUtc.HasValue)
                //    {
                //        ProcessPublishedEvent(contentEvent, unpublishLaterPart);
                //    }
                //}
                //else
                if (contentEvent.Name.Equals(Constants.ContentEvent_Saved, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (unpublishLaterPart.ScheduledUnpublishUtc.HasValue)
                    {
                        ProcessSavedEvent(contentEvent, unpublishLaterPart);
                    }
                }
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

        private static void ProcessPublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, UnpublishLaterPart unpublishLaterPart)
        {
            publishedContentEvent.Name = GetPublishedEventName(unpublishLaterPart);
        }

        private static void ProcessUnpublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, UnpublishLaterPart unpublishLaterPart)
        {
            publishedContentEvent.Name = GetUnpublishedEventName(unpublishLaterPart);
        }

        private static void ProcessSavedEvent(AuditTrailCreateContext<AuditTrailContentEvent> savedContentEvent, UnpublishLaterPart unpublishLaterPart)
        {
            savedContentEvent.Name = GetSavedEventName(unpublishLaterPart);
        }

        private static string GetPublishedEventName(UnpublishLaterPart unpublishLaterPart)
        {
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            return $"It...hed at ({scheduledUnpublishUtc})";
        }

        private static string GetUnpublishedEventName(UnpublishLaterPart unpublishLaterPart)
        {
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            return $"Item was sheduled unpublished at ({scheduledUnpublishUtc})";
        }

        private static string GetSavedEventName(UnpublishLaterPart unpublishLaterPart)
        {
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            return $"Item is to be unpublished at ({scheduledUnpublishUtc})";
        }
    }
}
