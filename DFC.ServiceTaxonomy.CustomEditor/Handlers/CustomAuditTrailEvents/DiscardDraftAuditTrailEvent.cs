using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers.CustomAuditTrailEvents
{
    public class DiscardDraftAuditTrailEvent : ICustomAuditTrailEvent
    {
        public void HandleCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            contentEvent.Name = Constants.AuditTrail.ContentEventName_DiscardDraft;
        }

        public bool ValidCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            return contentEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Removed, StringComparison.OrdinalIgnoreCase) &&
                    request.Path.Value.Contains(Constants.AuditTrail.UrlPart_DiscardDraft, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
