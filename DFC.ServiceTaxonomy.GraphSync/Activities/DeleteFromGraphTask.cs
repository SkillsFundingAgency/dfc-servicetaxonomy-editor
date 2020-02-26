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
//            return Outcomes(T["Done"], T["Failed"]);
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        //todo: if this fails, we notify the user, but the content still gets added to oc, and oc & the graph are then out-of-sync.
        // we need to think of the best way to handle it. the event appears to trigger after the content is created (check)
        // we don't want to remove the content in orchard core, as we don't want the user to have to reenter content
        // perhaps we could mark the content as not synced (part of the graph content part?), and either the user can retry
        // (and allow the user to filter by un-synced content)
        // or we have a facility to check & sync all content in oc
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                await _graphSyncer.DeleteFromGraph((ContentItem) workflowContext.Input["ContentItem"]);

                return Outcomes("Done");
            }
            catch (Exception ex)
            {
                // setting this, but not letting the exception propagate doesn't work
                //workflowContext.Fault(ex, activityContext);

//                _notifier.Add(new GetProperty<NotifyType>(), new LocalizedHtmlString(nameof(SyncToGraphTask), $"Sync to graph failed: {ex.Message}"));
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteFromGraphTask), $"Delete from graph failed: {ex.Message}"));

                // if we do this, we can trigger a notify task in the workflow from a failed outcome, but the workflow doesn't fault
                //return Outcomes("Failed");
                throw;
            }
        }
    }
}
