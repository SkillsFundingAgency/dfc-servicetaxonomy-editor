using DFC.ServiceTaxonomy.JobProfiles.Exporter.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IJobProfilesService _jobProfilesService;

        public AdminController(IAuthorizationService authorizationService, IJobProfilesService jobProfilesService)
        {
            _authorizationService = authorizationService;
            _jobProfilesService = jobProfilesService;
        }

        [Admin]
        [HttpGet]
        public async Task<IActionResult> TriggerExport()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageJobProfilesExporter))
            {
                return Forbid();
            }

            var csvStream = await _jobProfilesService.GenerateCsvStreamAsync();
            csvStream.Position = 0;

            string fileName = $"job_profiles_{DateTime.UtcNow:dd-MM-yyyy}.csv";
            return File(csvStream, "text/csv", fileName);
        }
    }
}
