using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

//todo: part handler called after workflow finishes - can we use that to stop inserts?

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class SyncToGraphTask : TaskActivity
    {
        public SyncToGraphTask(
            IMergeGraphSyncer mergeGraphSyncer,
            IStringLocalizer<SyncToGraphTask> localizer,
            INotifier notifier)
        {
            _mergeGraphSyncer = mergeGraphSyncer;
            _notifier = notifier;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        private readonly IMergeGraphSyncer _mergeGraphSyncer;
        private readonly INotifier _notifier;

        public override string Name => nameof(SyncToGraphTask);
        public override LocalizedString DisplayText => T["Sync content item to Neo4j graph"];
        public override LocalizedString Category => T["Graph"];

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
            string contentType = "unknown type";
            try
            {
                ContentItem contentItem = (ContentItem)workflowContext.Input["ContentItem"];

                // contentType is used in the catch block
#pragma warning disable S1854
                contentType = contentItem.ContentType;
#pragma warning restore S1854

                await _mergeGraphSyncer.SyncToGraph(
                    contentItem.ContentType,
                    contentItem.ContentItemId,
                    contentItem.ContentItemVersionId,
                    contentItem.Content,
                    contentItem.CreatedUtc,
                    contentItem.ModifiedUtc);

                return Outcomes("Done");
            }
            catch
            {
                // setting this, but not letting the exception propagate doesn't work
                //workflowContext.Fault(ex, activityContext);

                //todo: where's the best place for this code? the customnotifier? would need to dig more before putting it in its Add() method

                // the notifier blows up if there's any '{}' in the message, so we make the string safe for the notifier
                //string safeMessage = ex.Message.Replace('{','«').Replace('}', '»');

                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(SyncToGraphTask), $"Unable to sync {contentType} to graph."));

                // if we do this, we can trigger a notify task in the workflow from a failed outcome, but the workflow doesn't fault
                //return Outcomes("Failed");
                throw;
            }
        }
    }
}
