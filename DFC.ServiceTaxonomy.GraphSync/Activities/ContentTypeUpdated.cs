using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeUpdated : Activity
    {
        private readonly IStringLocalizer _localizer;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypeUpdated(IStringLocalizer localiszer, IContentDefinitionManager contentDefinitionManager)
        {
            _localizer = localiszer;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override string Name => "ContentTypeUpdated";

        public override LocalizedString DisplayText => _localizer["Content Type Updated Event"];

        public override LocalizedString Category => _localizer["ContentType"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            throw new NotImplementedException();
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            await Task.CompletedTask;
            return new ActivityExecutionResult(new List<string> { "Done" });
        }
    }
}
