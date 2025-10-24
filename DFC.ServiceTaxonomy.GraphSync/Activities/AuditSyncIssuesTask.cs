using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Enums;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
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
        private readonly ILogger<AuditSyncIssuesTask> _logger;
        private IStringLocalizer T { get; }

        public override string Name => nameof(AuditSyncIssuesTask);
        public override LocalizedString DisplayText => T["Identify sync issues and attempt repair"];
        public override LocalizedString Category => T["Graph"];

        public ValidationScope Scope
        {
            get => GetProperty(() => ValidationScope.ModifiedSinceLastValidation);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _logger.LogInformation("{AuditSyncIssuesTask} triggered.", nameof(AuditSyncIssuesTask));

            // Commented out to prevent from running but left in code in case needed later
            //await _validateAndRepairGraph.ValidateGraph(Scope);

            return Outcomes("Done");
        }
    }
}
