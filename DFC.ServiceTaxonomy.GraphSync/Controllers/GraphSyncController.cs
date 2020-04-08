using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class GraphSyncController : Controller
    {
        private readonly IValidateGraphSync _validateGraphSync;
        private readonly INotifier _notifier;
        private readonly ILogger<GraphSyncController> _logger;

        public GraphSyncController(
            IValidateGraphSync validateGraphSync,
            INotifier notifier,
            ILogger<GraphSyncController> logger)
        {
            _validateGraphSync = validateGraphSync;
            _notifier = notifier;
            _logger = logger;
        }

        [Admin]
        public async Task<IActionResult> TriggerSyncValidation()
        {
            try
            {
                //todo: add name of user who triggered
                //todo: double check user/permissions
                _logger.LogInformation("User sync validation triggered");
                await _validateGraphSync.ValidateGraph();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "User triggered sync validation failed: {e}");
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncController), $"Unable to verify graph sync."));
            }
            return View();
        }
    }
}
