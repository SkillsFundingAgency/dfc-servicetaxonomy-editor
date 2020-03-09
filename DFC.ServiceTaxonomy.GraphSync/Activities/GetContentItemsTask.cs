using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class GetContentItemsTask : TaskActivity
    {
        public GetContentItemsTask(
            ISession session,
            IStringLocalizer<GetContentItemsTask> localizer,
            IContentManager contentManager,
            INotifier notifier)
        {
            _notifier = notifier;
            _session = session;
            T = localizer;
            _contentManager = contentManager;
        }

        private IStringLocalizer T { get; }
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;

        public override string Name => nameof(GetContentItemsTask);
        public override LocalizedString DisplayText => T["Get content items by Content Type"];
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
            var contentTypeToSync = workflowContext.Input["ContentType"].ToString();
            var fieldsToAdd = workflowContext.Input["Added"] as List<string>;
            var fieldsToRemove = workflowContext.Input["Removed"] as List<string>;
            var query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeToSync);

            List<ContentItem> itemsToUpdate = new List<ContentItem>();

            if (fieldsToRemove.Any() || fieldsToAdd.Any())
            {
                foreach (var result in await query.ListAsync())
                {
                    itemsToUpdate.Add(await _contentManager.GetAsync(result.ContentItemId));
                }
            }

            foreach (var fieldToRemove in fieldsToRemove!)
            {
                foreach (var obj in itemsToUpdate)
                {
                    if (obj != null)
                    {
                        //TODO: Update ContentItem to remove field and publish

                        //For now, publish content change to trigger Sync to graph pipeline....
                        await _contentManager.PublishAsync(obj);
                    }
                }
            }

            await Task.Delay(0);
            return Outcomes("Done");
        }
    }
}

