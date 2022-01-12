using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.VersionComparison.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.VersionComparison.Drivers
{
    public class VersionComparisonContentsDriver : ContentDisplayDriver
    {
        private readonly IAuditTrailQueryService _auditTrailQueryService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VersionComparisonContentsDriver(IAuditTrailQueryService auditTrailQueryService, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _auditTrailQueryService = auditTrailQueryService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var results = new List<IDisplayResult>();
            var hasAuditTrailPermissions = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AuditTrailPermissions.ViewAuditTrail, model);
            if(!hasAuditTrailPermissions)
            {
                return Combine(results.ToArray());
            }

            var versions = await _auditTrailQueryService.GetVersions(model.ContentItemId);

            if (versions.Count > 1)
            {
                var compareVersionsButton = Initialize<ContentItemViewModel>("VersionComparisonContentsAction_SummaryAdmin", m => m.ContentItem = model).Location("SummaryAdmin", "ActionsMenu:10");
                results.Add(compareVersionsButton);
            }

            return Combine(results.ToArray());
        }
    }
}
