using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DeleteFromGraphTask : TaskActivity
    {
        public DeleteFromGraphTask(
            IDeleteGraphSyncer deleteGraphSyncer,
            ISession session,
            IStringLocalizer<DeleteFromGraphTask> localizer,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager,
            IPublishedContentItemVersion publishedContentItemVersion)
        {
            _deleteGraphSyncer = deleteGraphSyncer;
            _session = session;
            _notifier = notifier;
            T = localizer;
            _contentDefinitionManager = contentDefinitionManager;
            _publishedContentItemVersion = publishedContentItemVersion;
        }

        private IStringLocalizer T { get; }
        private readonly IDeleteGraphSyncer _deleteGraphSyncer;
        private readonly ISession _session;
        private readonly INotifier _notifier;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;

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
                await _deleteGraphSyncer.Delete(contentItem, _publishedContentItemVersion);
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message != "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    _session.Cancel();
                    AddFailureNotifier(contentItem);
                    throw;
                }
            }
            catch
            {
                _session.Cancel();
                AddFailureNotifier(contentItem);
                throw;
            }

            return Outcomes("Done");
        }

        private void AddFailureNotifier(ContentItem contentItem)
        {
            string contentType = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;

            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteFromGraphTask),
                $"The {contentType} could not be removed because the associated node could not be deleted from the graph."));
        }
    }
}
