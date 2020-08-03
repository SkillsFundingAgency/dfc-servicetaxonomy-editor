using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DeleteContentTypeTask : TaskActivity
    {
        public DeleteContentTypeTask(
            IStringLocalizer<DeleteContentTypeTask> localizer,
            INotifier notifier,
            IOrchardCoreContentDefinitionService contentDefinitionService,
            ILogger<DeleteContentTypeTask> logger)
        {
            _notifier = notifier;
            T = localizer;
            _contentDefinitionService = contentDefinitionService;
            Logger = logger;
        }

        private IStringLocalizer T { get; }
        private readonly INotifier _notifier;
        private readonly IOrchardCoreContentDefinitionService _contentDefinitionService;

        public ILogger Logger { get; }

        public override string Name => nameof(DeleteContentTypeTask);
        public override LocalizedString DisplayText => T["Delete content type from Orchard Core"];
        public override LocalizedString Category => T["Content Type"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            string? typeToDelete = workflowContext.Input["ContentType"].ToString();

            if (string.IsNullOrWhiteSpace(typeToDelete))
                throw new ArgumentNullException($"No Content Type passed to {nameof(DeleteContentTypeTask)}");

            try
            {
                _contentDefinitionService.RemoveType(typeToDelete, true);

                return Outcomes("Done");
            }
            catch
            {
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteContentTypeTask), $"The {typeToDelete} could not be removed."));
                throw;
            }
        }
    }
}
