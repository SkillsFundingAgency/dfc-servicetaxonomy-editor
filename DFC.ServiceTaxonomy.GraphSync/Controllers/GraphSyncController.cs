using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class GraphSyncController : Controller
    {
        private readonly IValidateAndRepairGraph _validateAndRepairGraph;
        private readonly IAuthorizationService _authorizationService;
        private readonly IGraphSyncNotifier _notifier;
        private readonly ILogger<GraphSyncController> _logger;
        private readonly ISynonymService _synonymService;

        public GraphSyncController(
            IValidateAndRepairGraph validateAndRepairGraph,
            IAuthorizationService authorizationService,
            IGraphSyncNotifier notifier,
            ILogger<GraphSyncController> logger,
            ISynonymService synonymService)
        {
            _validateAndRepairGraph = validateAndRepairGraph;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _logger = logger;
            _synonymService = synonymService;
        }

        [HttpGet("GraphSync/Synonyms/{node}/{filename}")]
        public async Task<IActionResult> Synonyms(string node, string filename)
        {
            var synonyms = await _synonymService.GetSynonymsAsync(node);

            var sb = new StringBuilder();
            foreach (var item in synonyms)
            {
                sb.AppendLine(item);
            }

            var stream = new MemoryStream(buffer: Encoding.UTF8.GetBytes(sb.ToString()));

            try
            {
                stream.Position = 0;
                return new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = filename
                };
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        [Admin]
        public async Task<IActionResult> TriggerSyncValidation(ValidationScope scope)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AdministerGraphs))
            {
                return Forbid();
            }

            IValidateAndRepairResults? validateAndRepairResults = null;
            try
            {
                //todo: display page straight away : show progress (log) in page
                //todo: add name of user who triggered into logs (if not already sussable)
                _logger.LogInformation("User sync validation triggered");
                validateAndRepairResults = await _validateAndRepairGraph.ValidateGraph(scope);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "User triggered sync validation failed.");
                _notifier.Add("Unable to validate graph sync.", exception: e);
            }
            return View(new TriggerSyncValidationViewModel
            {
                ValidateAndRepairResults = validateAndRepairResults
            });
        }
    }
}
