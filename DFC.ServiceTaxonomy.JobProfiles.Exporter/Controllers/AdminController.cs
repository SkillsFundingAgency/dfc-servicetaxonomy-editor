using DFC.ServiceTaxonomy.JobProfiles.Exporter.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IJobProfilesService _jobProfilesService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAuthorizationService authorizationService, IJobProfilesService jobProfilesService, ILogger<AdminController> logger)
        {
            _authorizationService = authorizationService;
            _jobProfilesService = jobProfilesService;
            _logger = logger;
        }

        [Admin]
        [HttpGet]
        public async Task<IActionResult> TriggerExport()
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(TriggerExport));

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageJobProfilesExporter))
            {
                _logger.LogInformation("The user does not have permission {Permission}", nameof(Permissions.ManageJobProfilesExporter));
                return Forbid();
            }

            string fileName = $"job_profiles_{DateTime.UtcNow:dd-MM-yyyy}.csv";

            var stream = await _jobProfilesService.GenerateCsvStreamAsync();
            stream.Position = 0;

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(TriggerExport));
            return File(stream, "text/csv", fileName);
        }
    }
}
