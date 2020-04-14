using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class AuditSyncIssuesTask : TaskActivity
    {
        public AuditSyncIssuesTask(
            IValidateAndRepairGraph validateAndRepairGraph,
            IStringLocalizer<AuditSyncIssuesTask> localizer,
            ILogger<AuditSyncIssuesTask> logger)
        {
            _validateAndRepairGraph = validateAndRepairGraph;
            _logger = logger;
            T = localizer;
        }

        private readonly IValidateAndRepairGraph _validateAndRepairGraph;
        private readonly ILogger _logger;
        private IStringLocalizer T { get; }

        public override string Name => nameof(AuditSyncIssuesTask);
        public override LocalizedString DisplayText => T["Identify sync issues and attempt repair"];
        public override LocalizedString Category => T["Graph"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _logger.LogInformation($"{nameof(AuditSyncIssuesTask)} triggered");

            await _validateAndRepairGraph.ValidateGraph();

            return Outcomes("Done");
        }
    }
}
