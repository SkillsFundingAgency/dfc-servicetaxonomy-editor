using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

//todo: part handler called after workflow finishes - can we use that to stop inserts?

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DeleteContentTypeTask : TaskActivity
    {
        public DeleteContentTypeTask(
            IStringLocalizer<DeleteFromGraphTask> localizer,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager)
        {
            _notifier = notifier;
            T = localizer;
            _contentDefinitionManager = contentDefinitionManager;
        }

        private IStringLocalizer T { get; }
        private readonly INotifier _notifier;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public override string Name => nameof(DeleteContentTypeTask);
        public override LocalizedString DisplayText => T["Delete content type from Orchard Core"];
        public override LocalizedString Category => T["Content Type"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            await Task.Delay(0);
            var typeToDelete = workflowContext.Input["ContentType"].ToString();

            if (string.IsNullOrWhiteSpace(typeToDelete))
                throw new InvalidOperationException($"No Content Type passed to {nameof(DeleteContentTypeTask)}");

            try
            {
                // first remove all attached parts
                var typeDefinition = _contentDefinitionManager.LoadTypeDefinition(typeToDelete);
                var partDefinitions = typeDefinition.Parts.ToArray();
                foreach (var partDefinition in partDefinitions)
                {
                    _contentDefinitionManager.AlterTypeDefinition(typeDefinition.Name, typeBuilder => typeBuilder.RemovePart(partDefinition.Name));

                    // delete the part if it's its own part
                    if (partDefinition.PartDefinition.Name == typeDefinition.Name)
                    {
                        RemovePart(partDefinition.Name);
                    }
                }

                _contentDefinitionManager.DeleteTypeDefinition(typeDefinition.Name);
                
                return Outcomes("Done");
            }
            catch
            {
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteContentTypeFromGraphTask), $"The {typeToDelete} could not be removed from Orchard Core."));
                throw;
            }
        }

        private void RemovePart(string name)
        {
            var partDefinition = _contentDefinitionManager.LoadPartDefinition(name);

            if (partDefinition == null)
            {
                // Couldn't find this named part, ignore it
                return;
            }

            var fieldDefinitions = partDefinition.Fields.ToArray();
            foreach (var fieldDefinition in fieldDefinitions)
            {
                RemoveFieldFromPart(fieldDefinition.Name, name);
            }

            _contentDefinitionManager.DeletePartDefinition(name);
        }

        private void RemoveFieldFromPart(string fieldName, string partName)
        {
            _contentDefinitionManager.AlterPartDefinition(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
        }
    }
}
