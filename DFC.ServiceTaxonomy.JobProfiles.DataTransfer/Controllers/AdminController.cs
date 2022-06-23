
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OrchardCore.Admin;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        private readonly IReIndexService _reIndexService;

        public AdminController(IAuthorizationService authorizationService, IReIndexService reIndexService)
        {
            _authorizationService = authorizationService;
            _reIndexService = reIndexService;
        }

        [Admin]
        [HttpGet]
        public async Task<IActionResult> TriggerReindex()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAzureSearchIndex))
            {
                return Forbid();
            }

            await _reIndexService.ReIndexAsync();

            return View();
        }
    }
}
