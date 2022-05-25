using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers.CustomAuditTrailEvents
{
    public interface ICustomAuditTrailEvent
    {
        bool ValidCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request);

        void HandleCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request);
    }
}
