using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public AdminController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [Admin]
        [HttpGet]
        public async Task<IActionResult> TriggerExport()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageJobProfilesExporter))
            {
                return Forbid();
            }

            return View();
        }
    }
}
