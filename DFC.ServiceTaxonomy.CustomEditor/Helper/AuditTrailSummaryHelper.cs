using System;
using System.Linq;
using DFC.ServiceTaxonomy.CustomEditor.Constants;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.PublishLater.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Helper
{
    public static class AuditTrailSummaryHelper
    {
        public static string GetAuditTrailItemSummaryTextFormat(AuditTrailContentEventViewModel auditTrailContentEventViewModel)
        {
            if (unpublishedEvents.Any(ev => auditTrailContentEventViewModel.AuditTrailEvent.Name.Equals(ev, StringComparison.InvariantCultureIgnoreCase)))
            {
                return GetUnpublishLaterSummaryFormat(auditTrailContentEventViewModel);
            }
            if(publishedEvents.Any(ev => auditTrailContentEventViewModel.AuditTrailEvent.Name.Equals(ev, StringComparison.InvariantCultureIgnoreCase)))
            {
                return GetPublishLaterSummaryFormat(auditTrailContentEventViewModel);
            }
            if(auditTrailContentEventViewModel.AuditTrailEvent.Name.Equals(ContentAuditTrailEventConfiguration.Restored, StringComparison.InvariantCultureIgnoreCase))
            {
                return $"The {auditTrailContentEventViewModel.ContentItem.ContentType} {{1}} was restored as {{0}}";
            }
            if (auditTrailContentEventViewModel.AuditTrailEvent.Name == ContentAuditTrailEventConfiguration.Removed)
            {
                var contentItem = auditTrailContentEventViewModel.ContentItem;
                return $"The {contentItem.ContentType} {contentItem.DisplayText} was removed.";
            }
            return string.Empty;
        }

        private static readonly string[] publishedEvents = new[] { AuditTrail.ContentEvent_Publish_Later, AuditTrail.ContentEvent_Publish_Later_Success, AuditTrail.ContentEvent_Publish_Later_Cancelled };

        private static string GetPublishLaterSummaryFormat(AuditTrailContentEventViewModel auditTrailContentEventViewModel)
        {
            var contentItem = auditTrailContentEventViewModel.ContentItem;
            var publishLaterPart = contentItem.ContentItem.As<PublishLaterPart>();
            if (publishLaterPart == null)
            {
                return string.Empty;
            }
            var scheduledPublishUtc = publishLaterPart.ScheduledPublishUtc.HasValue ? publishLaterPart.ScheduledPublishUtc.Value.ToString() : "no value";
            switch (auditTrailContentEventViewModel.AuditTrailEvent.Name)
            {
                case AuditTrail.ContentEvent_Publish_Later:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was scheduled to be published at {scheduledPublishUtc}";
                case AuditTrail.ContentEvent_Publish_Later_Cancelled:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} cancelled a previously scheduled publish";
                case AuditTrail.ContentEvent_Publish_Later_Success:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was published as per schedule at {scheduledPublishUtc}";
                default:
                    return string.Empty;
            }

        }

        private static readonly string[] unpublishedEvents = new[] { AuditTrail.ContentEvent_Unpublish_Later, AuditTrail.ContentEvent_Unpublish_Later_Success, AuditTrail.ContentEvent_Unpublish_Later_Cancelled };

        private static string GetUnpublishLaterSummaryFormat(AuditTrailContentEventViewModel auditTrailContentEventViewModel)
        {
            var contentItem = auditTrailContentEventViewModel.ContentItem;
            var unpublishLaterPart = contentItem.ContentItem.As<UnpublishLaterPart>();
            if (unpublishLaterPart == null)
            {
                return string.Empty;
            }
            var scheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc.HasValue ? unpublishLaterPart.ScheduledUnpublishUtc.Value.ToString() : "no value";
            switch (auditTrailContentEventViewModel.AuditTrailEvent.Name)
            {
                case AuditTrail.ContentEvent_Unpublish_Later:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was scheduled to be unpublished at {scheduledUnpublishUtc}";
                case AuditTrail.ContentEvent_Unpublish_Later_Cancelled:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} cancelled a previously scheduled unpublish";
                case AuditTrail.ContentEvent_Unpublish_Later_Success:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was unpublished as per schedule at {scheduledUnpublishUtc}";
                default:
                    return string.Empty;
            }
        }
    }
}
