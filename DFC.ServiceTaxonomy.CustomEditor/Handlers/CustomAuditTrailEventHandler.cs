using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers
{
    public class CustomAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomAuditTrailEventHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> removedEvent &&
                removedEvent.Name.Equals(Constants.AuditTrail.ContentEvent_Removed, StringComparison.CurrentCultureIgnoreCase)
                )
            {
                // No way to divine whether a Remove event is a delete or discarded draft from the content item so need to look a the URL.
                var isDiscard = _httpContextAccessor.HttpContext!.Request.Path.Value!.Contains(Constants.AuditTrail.UrlPart_DiscardDraft, StringComparison.CurrentCultureIgnoreCase);
                if (isDiscard)
                {
                    removedEvent.Name = Constants.AuditTrail.ContentEventName_DiscardDraft;
                }
            }
            return Task.CompletedTask;
        }
    }
}
