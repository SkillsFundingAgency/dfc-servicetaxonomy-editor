using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql;
using OrchardCore.ContentManagement.Records;
using System;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class PublishContentTypeContentItemsTask : TaskActivity
    {
        public PublishContentTypeContentItemsTask(
            ISession session,
            IStringLocalizer<RemoveFieldFromContentItemsTask> localizer,
            IContentManager contentManager,
             ITypeActivatorFactory<ContentPart> contentPartFactory)
        {
            _session = session;
            T = localizer;
            _contentManager = contentManager;
            _contentPartFactory = contentPartFactory;
        }

        private IStringLocalizer T { get; }
        private readonly ISession _session;
        private readonly IContentManager _contentManager;

        public override string Name => nameof(PublishContentTypeContentItemsTask);
        public override LocalizedString DisplayText => T["Publish all Content Items for a Content Type"];
        public override LocalizedString Category => T["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            //            return Outcomes(T["Done"], T["Failed"]);
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            string contentTypeToSync = GetContentTypeFromWorkflowContext(workflowContext);

            var query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeToSync);

            foreach (var contentItem in await query.ListAsync())
            {
                var item = await _contentManager.GetAsync(contentItem.ContentItemId);

                //Force the item to be published
                item.Published = false;
                await _contentManager.PublishAsync(item);
            }

            return Outcomes("Done");
        }

        private static string GetContentTypeFromWorkflowContext(WorkflowExecutionContext workflowContext)
        {
            var contentTypeToSync = workflowContext.Input["ContentType"].ToString();

            if (string.IsNullOrWhiteSpace(contentTypeToSync))
                throw new ArgumentException($"Content Type not passed to {Name}");
            return contentTypeToSync;
        }
    }
}

