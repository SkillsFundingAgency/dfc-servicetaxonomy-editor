using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomEditor.Handlers.CustomAuditTrailEvents;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers
{
    public class CustomAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        private readonly IHttpContextAccessor  _httpContextAccessor;
        private readonly IEnumerable<ICustomAuditTrailEvent> _customAuditTrailEvents;

        public CustomAuditTrailEventHandler(IHttpContextAccessor httpContextAccessor, IEnumerable<ICustomAuditTrailEvent> customAuditTrailEvents)
        {
            _httpContextAccessor = httpContextAccessor;
            _customAuditTrailEvents = customAuditTrailEvents;
        }

        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent)
            {
                var request = _httpContextAccessor.HttpContext.Request;
                var customEvent = _customAuditTrailEvents.FirstOrDefault(ev => ev.ValidCustomEvent(contentEvent, request));
                if(customEvent != null)
                {
                    customEvent.HandleCustomEvent(contentEvent, request);
                }
            }

            return Task.CompletedTask;
        }
    }
}
