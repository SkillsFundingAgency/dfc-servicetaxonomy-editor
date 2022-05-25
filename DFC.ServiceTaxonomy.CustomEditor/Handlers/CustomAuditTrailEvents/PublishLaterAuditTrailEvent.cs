using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.PublishLater.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers.CustomAuditTrailEvents
{
    public class PublishLaterAuditTrailEvent : ICustomAuditTrailEvent
    {
        public void HandleCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            var requestForm = request.HasFormContentType ? request.Form : new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            if (contentEvent.Name == Constants.AuditTrail.ContentEvent_Saved)
            {
                contentEvent.Name = requestForm[Constants.AuditTrail.Save_Button_Key] == Constants.AuditTrail.Cancel_Publish_Later ?
                    Constants.AuditTrail.ContentEvent_Publish_Later_Cancelled :
                    Constants.AuditTrail.ContentEvent_Publish_Later;
            }
            else if (contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase))
            {
                contentEvent.Name = Constants.AuditTrail.ContentEvent_Publish_Later_Success;
            }
        }

        public bool ValidCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            var requestForm = request.HasFormContentType ? request.Form : new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            // If no PublishLaterPart then ignore
            var publishLaterPart = contentEvent.AuditTrailEventItem.ContentItem.As<PublishLaterPart>();
            if (publishLaterPart == null)
            {
                return false;
            }
            // If saved and was a cancel publish later button click or a scheduled publish time has been set then handle custom event
            if (contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Saved, StringComparison.InvariantCultureIgnoreCase) &&
                (requestForm[Constants.AuditTrail.Save_Button_Key] == Constants.AuditTrail.Cancel_Publish_Later ||
                publishLaterPart.ScheduledPublishUtc.HasValue))
            {
                return true;
            }
            // If published and a scheduled publish time is present then handle event
            if (contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase) &&
                publishLaterPart.ScheduledPublishUtc.HasValue)
            {
                return true;
            }

            return false;
        }
    }
}
