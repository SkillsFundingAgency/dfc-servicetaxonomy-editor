using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class GraphSyncController : Controller
    {
        [Admin]
        public IActionResult TriggerSyncValidation()
        {
            return View();
        }
    }
}
