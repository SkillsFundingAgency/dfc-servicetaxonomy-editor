using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class GraphSyncController : Controller
    {
        private readonly IValidateAndRepairGraph _validateAndRepairGraph;
        private readonly INotifier _notifier;
        private readonly ILogger<GraphSyncController> _logger;

        public GraphSyncController(
            IValidateAndRepairGraph validateAndRepairGraph,
            INotifier notifier,
            ILogger<GraphSyncController> logger)
        {
            _validateAndRepairGraph = validateAndRepairGraph;
            _notifier = notifier;
            _logger = logger;
        }

        [Admin]
        public async Task<IActionResult> TriggerSyncValidation()
        {
            bool valiadtionSuccess = false;
            try
            {
                //todo: display page straight away : show progress (log) in page
                //todo: add name of user who triggered into logs (if not already sussable)
                //todo: double check user/permissions
                _logger.LogInformation("User sync validation triggered");
                valiadtionSuccess = await _validateAndRepairGraph.ValidateGraph();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "User triggered sync validation failed: {e}");
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncController), $"Unable to verify graph sync."));
            }
            return View(new TriggerSyncValidationViewModel
            {
                ValidationSuccess = valiadtionSuccess
            });
        }
    }
}
