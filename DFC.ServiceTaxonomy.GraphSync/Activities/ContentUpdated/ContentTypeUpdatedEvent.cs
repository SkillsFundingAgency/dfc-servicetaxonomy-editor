using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeUpdatedEvent : Activity, IEvent
    {
        public ContentTypeUpdatedEvent(IContentDefinitionManager contentDefinitionManager, IWorkflowScriptEvaluator scriptEvaluator)
        {
            ContentDefinitionManager = contentDefinitionManager;
            ScriptEvaluator = scriptEvaluator;
        }

        protected IContentDefinitionManager ContentDefinitionManager { get; }

        protected IWorkflowScriptEvaluator ScriptEvaluator { get; }

        public override LocalizedString Category => new LocalizedString("Content Type", "Content Type");

        public override string Name => "ContentTypeUpdatedEvent";

        public override LocalizedString DisplayText => new LocalizedString("Content Type Updated", "Content Type Updated");

#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(new LocalizedString("Done", "Done"));
        }
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}
