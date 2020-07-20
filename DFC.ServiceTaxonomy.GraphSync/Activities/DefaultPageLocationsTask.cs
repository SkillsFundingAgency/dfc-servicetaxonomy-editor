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
using System.Linq;
using GraphQL;
using OrchardCore.ContentManagement.Workflows;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class DefaultPageLocationsTask : TaskActivity
    {
        public DefaultPageLocationsTask(
            ISession session,
            IStringLocalizer<DefaultPageLocationsTask> localizer,
            IContentManager contentManager)
        {
            _session = session;
            T = localizer;
            _contentManager = contentManager;
        }

        private IStringLocalizer T { get; }
        private readonly ISession _session;
        private readonly IContentManager _contentManager;

        public override string Name => nameof(DefaultPageLocationsTask);
        public override LocalizedString DisplayText => T["Publish all Content Items for a Content Type"];
        public override LocalizedString Category => T["Pages"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            //            return Outcomes(T["Done"], T["Failed"]);
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentEvent = workflowContext.Input["ContentEvent"].As<ContentEventContext>();
            var contentItem = workflowContext.Input["ContentItem"].As<ContentItem>();

            if (Convert.ToBoolean(contentItem.Content.Page.DefaultPageAtLocation.Value))
            {
                //TODO : find out how to query for only pages marked as default - probably need to make a new index
                var pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.Latest && x.ContentType == contentEvent.ContentType).ListAsync();

                foreach (var page in pages.Where(x => x.ContentItemId != contentEvent.ContentItemId && x.Content.Page.PageLocations.TagNames[0] == contentItem.Content.Page.PageLocations.TagNames[0]))
                {
                    if (contentEvent.Name == "ContentPublishedEvent")
                    {
                        var latestPublished = await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Published);

                        if (latestPublished != null)
                        {
                            latestPublished.Content.Page.DefaultPageAtLocation.Value = false;
                            latestPublished.Published = false;
                            await _contentManager.PublishAsync(latestPublished);
                        }
                    }

                    var contentItemVersion = new ContentItemVersion(GraphReplicaSetNames.Preview);
                    (bool? latest, bool? published) = contentItemVersion.ContentItemIndexFilterTerms;

                    var drafts = await _session.Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == contentEvent.ContentType &&
                        (latest == null || x.Latest == latest) &&
                        (published == null || x.Published == published)).ListAsync();

                    foreach (var draft in drafts.Where(x => x.ContentItemId != contentEvent.ContentItemId && x.Content.Page.PageLocations.TagNames[0] == contentItem.Content.Page.PageLocations.TagNames[0]))
                    {
                        var draftContentItem = await contentItemVersion.GetContentItemAsync(_contentManager, draft.ContentItemId);

                        if (draftContentItem != null)
                        {
                            draftContentItem.Content.Page.DefaultPageAtLocation.Value = false;
                            await _contentManager.SaveDraftAsync(draftContentItem);
                        }
                    }
                }
            }

            return Outcomes("Done");
        }
    }
}

