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
        public ContentTypeUpdatedEvent(IContentDefinitionManager contentDefinitionManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentTypeUpdatedEvent> localizer)
        {
            ContentDefinitionManager = contentDefinitionManager;
            ScriptEvaluator = scriptEvaluator;
            S = localizer;
        }

        protected IStringLocalizer<ContentTypeUpdatedEvent> S { get; }

        protected IContentDefinitionManager ContentDefinitionManager { get; }

        protected IWorkflowScriptEvaluator ScriptEvaluator { get; }

        public override LocalizedString Category => new LocalizedString("Content Type", "Content Type");

        public override string Name => nameof(ContentTypeUpdatedEvent);

        public override LocalizedString DisplayText => S["Content Type Updated"];

#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext) => Outcomes(S["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}
