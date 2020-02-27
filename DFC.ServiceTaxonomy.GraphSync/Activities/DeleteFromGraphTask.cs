using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

//todo: part handler called after workflow finishes - can we use that to stop inserts?
//todo: content delete

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DeleteFromGraphTask : TaskActivity
    {
        public DeleteFromGraphTask(
            IGraphSyncer graphSyncer,
            IStringLocalizer<DeleteFromGraphTask> localizer,
            INotifier notifier)
        {
            _graphSyncer = graphSyncer;
            _notifier = notifier;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        private readonly IGraphSyncer _graphSyncer;
        private readonly INotifier _notifier;

        public override string Name => nameof(DeleteFromGraphTask);
        public override LocalizedString DisplayText => T["Delete content item from Neo4j graph"];
        public override LocalizedString Category => T["National Careers Service"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                await _graphSyncer.DeleteFromGraph((ContentItem) workflowContext.Input["ContentItem"]);

                return Outcomes("Done");
            }
            catch (Exception ex)
            {
                //TODO : the exception message isn't very user friendly, need to use something better?
                //TODO : find out how to hide the success message, the notifier doesn't provide a means of clearing the existing notifications
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteFromGraphTask), $"Delete from graph failed: {ex.Message}"));
                throw;
            }
        }
    }
}
