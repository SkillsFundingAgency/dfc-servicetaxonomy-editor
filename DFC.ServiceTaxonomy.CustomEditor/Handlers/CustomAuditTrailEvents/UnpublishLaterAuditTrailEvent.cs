using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.ViewModels;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers.CustomAuditTrailEvents
{
    public class UnpublishLaterAuditTrailEvent : ICustomAuditTrailEvent
    {
        public void HandleCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            var requestForm = request.HasFormContentType ? request.Form : new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            if(contentEvent.Name == Constants.AuditTrail.ContentEvent_Published)
            {
                contentEvent.Name = requestForm[Constants.AuditTrail.Publish_Button_Key] == Constants.AuditTrail.Cancel_Unpublish_Later ?
                    Constants.AuditTrail.ContentEvent_Unpublish_Later_Cancelled :
                    Constants.AuditTrail.ContentEvent_Unpublish_Later;
            }
            else if(contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Unpublished, StringComparison.InvariantCultureIgnoreCase))
            {
                contentEvent.Name = Constants.AuditTrail.ContentEvent_Unpublish_Later_Success;
            }
        }

        public bool ValidCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            var requestForm = request.HasFormContentType ? request.Form : new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            // If no UnpublishLaterPart then ignore
            var unpublishLaterPart = contentEvent.AuditTrailEventItem.ContentItem.As<UnpublishLaterPart>();
            if (unpublishLaterPart == null)
            {
                return false;
            }
            // If published and was a cancel unpublish later button click or a scheduled unpublish time has been set then handle custom event
            if(contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase) &&
                (requestForm[Constants.AuditTrail.Publish_Button_Key] == Constants.AuditTrail.Cancel_Unpublish_Later ||
                unpublishLaterPart.ScheduledUnpublishUtc.HasValue))
            {
                return true;
            }
            // If unpublished and a scheduled unpublish time is present then handle event
            if(contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Unpublished, StringComparison.InvariantCultureIgnoreCase) &&
                unpublishLaterPart.ScheduledUnpublishUtc.HasValue)
            {
                return true;
            }

            return false;
        }

        public static readonly string[] unpublishedEvents = new[] { Constants.AuditTrail.ContentEvent_Unpublish_Later, Constants.AuditTrail.ContentEvent_Unpublish_Later_Success, Constants.AuditTrail.ContentEvent_Unpublish_Later_Cancelled };

        public static string GetAuditTrailSummaryFormat(AuditTrailContentEventViewModel auditTrailContentEventViewModel)
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
                case Constants.AuditTrail.ContentEvent_Unpublish_Later:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was scheduled to be unpublished at {scheduledUnpublishUtc}";
                case Constants.AuditTrail.ContentEvent_Unpublish_Later_Cancelled:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} cancelled a previously scheduled unpublish";
                case Constants.AuditTrail.ContentEvent_Unpublish_Later_Success:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was unpublished as per schedule at {scheduledUnpublishUtc}";
                default:
                    return $"{{0}} of the {contentItem.ContentType} {{1}} was {auditTrailContentEventViewModel.AuditTrailEvent.Name.ToLower()}";
            }

        }
    }
}
