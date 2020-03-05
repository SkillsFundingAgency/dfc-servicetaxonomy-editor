using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

//todo: part handler called after workflow finishes - can we use that to stop inserts?

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DeleteFromGraphTask : TaskActivity
    {
        public DeleteFromGraphTask(
            IDeleteGraphSyncer deleteGraphSyncer,
            IStringLocalizer<DeleteFromGraphTask> localizer,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager)
        {
            _deleteGraphSyncer = deleteGraphSyncer;
            _notifier = notifier;
            T = localizer;
            _contentDefinitionManager = contentDefinitionManager;
        }

        private IStringLocalizer T { get; }
        private readonly IDeleteGraphSyncer _deleteGraphSyncer;
        private readonly INotifier _notifier;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public override string Name => nameof(DeleteFromGraphTask);
        public override LocalizedString DisplayText => T["Delete content item from Neo4j graph"];
        public override LocalizedString Category => T["Graph"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItem = (ContentItem)workflowContext.Input["ContentItem"];

            try
            {
                await _deleteGraphSyncer.DeleteFromGraph(contentItem);

                return Outcomes("Done");
            }
            catch
            {
                var contentType = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
                //TODO : find out how to hide the success message, the notifier doesn't provide a means of clearing the existing notifications
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteFromGraphTask), $"The {contentType} could not be removed because the associated node could not be deleted from the graph."));
                throw;
            }
        }
    }
}
