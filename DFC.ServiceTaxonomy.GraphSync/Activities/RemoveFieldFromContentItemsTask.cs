using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class RemoveFieldFromContentItemsTask : TaskActivity
    {
        public RemoveFieldFromContentItemsTask(
            ISession session,
            IStringLocalizer<RemoveFieldFromContentItemsTask> localizer,
            IContentDefinitionManager contentManager,
            INotifier notifier,
            IOrchardCoreContentDefinitionService contentDefinitionService,
             ITypeActivatorFactory<ContentPart> contentPartFactory)
        {
            T = localizer;
            _contentDefinitionService = contentDefinitionService;
        }

        private IStringLocalizer T { get; }
        private readonly IOrchardCoreContentDefinitionService _contentDefinitionService;

        public override string Name => nameof(RemoveFieldFromContentItemsTask);
        public override LocalizedString DisplayText => T["Remove field from a Content Type"];
        public override LocalizedString Category => T["Content"];

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
            string? contentTypeToSync, fieldToRemove;
            ValidateParameters(workflowContext, out contentTypeToSync, out fieldToRemove);

            _contentDefinitionService.RemoveFieldFromPart(fieldToRemove, contentTypeToSync);

            return Outcomes("Done");
        }

        private void ValidateParameters(WorkflowExecutionContext workflowContext, out string? contentTypeToSync, out string? fieldToRemove)
        {
            contentTypeToSync = workflowContext.Input["ContentType"].ToString();
            if (string.IsNullOrWhiteSpace(contentTypeToSync))
                throw new ArgumentException($"Content Type not passed to {Name}");

            fieldToRemove = workflowContext.Input["RemovedField"].ToString();
            if (string.IsNullOrWhiteSpace(fieldToRemove))
                throw new ArgumentException($"RemovedField not passed to {Name}");
        }
    }
}

