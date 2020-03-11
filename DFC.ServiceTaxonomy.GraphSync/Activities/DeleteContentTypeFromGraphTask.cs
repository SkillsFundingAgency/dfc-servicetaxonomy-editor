using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

//todo: part handler called after workflow finishes - can we use that to stop inserts?

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DeleteContentTypeFromGraphTask : TaskActivity
    {
        public DeleteContentTypeFromGraphTask(
            IDeleteGraphSyncer deleteGraphSyncer,
            IStringLocalizer<DeleteFromGraphTask> localizer,
            INotifier notifier)
        {
            _deleteGraphSyncer = deleteGraphSyncer;
            _notifier = notifier;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        private readonly IDeleteGraphSyncer _deleteGraphSyncer;
        private readonly INotifier _notifier;

        public override string Name => nameof(DeleteContentTypeFromGraphTask);
        public override LocalizedString DisplayText => T["Delete content type from Neo4j graph"];
        public override LocalizedString Category => T["Graph"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var typeToDelete = workflowContext.Input["ContentType"].ToString();

            if (string.IsNullOrWhiteSpace(typeToDelete))
                throw new InvalidOperationException($"No Content Type passed to {nameof(DeleteContentTypeFromGraphTask)}");

            try
            {
                //Delete all nodes by type
                await _deleteGraphSyncer.DeleteNodesByType(typeToDelete);
                return Outcomes("Done");
            }
            catch (Exception)
            {
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteContentTypeFromGraphTask), $"Error: The {typeToDelete} could not be removed because the associated node could not be deleted from the graph. Most likely due to {typeToDelete} having incoming relationships."));

                throw;
            }
        }
    }
}
