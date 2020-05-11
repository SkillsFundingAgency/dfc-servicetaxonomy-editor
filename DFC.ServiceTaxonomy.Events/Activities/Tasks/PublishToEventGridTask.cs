using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.Events.Activities.Tasks
{
    public class PublishToEventGridTask : TaskActivity
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly IContentManager _contentManager;
        private readonly ILogger<PublishToEventGridTask> _logger;

        public PublishToEventGridTask(
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            IContentManager contentManager,
            IStringLocalizer<PublishToEventGridTask> localizer,
            ILogger<PublishToEventGridTask> logger)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
            _contentManager = contentManager;
            _logger = logger;
            T = localizer;
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            #pragma warning disable S3220
            return Outcomes(T["Done"]);
            #pragma warning restore S3220
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(PublishToEventGridTask);
        public override LocalizedString DisplayText => T["Publish content state changes to Azure Event Grid"];
        public override LocalizedString Category => T["Event Grid"];

        public override async Task<ActivityExecutionResult> ExecuteAsync(
            WorkflowExecutionContext workflowContext,
            ActivityContext activityContext)
        {
            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return Outcomes("Done");
            }

            ContentItem contentItem = (ContentItem)workflowContext.Input["ContentItem"];

            string? eventType = null;

            string trigger = (string)workflowContext.Properties["Trigger"];
            switch (trigger)
            {
                case "published":
                    eventType = "published";
                    break;
                case "updated":
                    if (!contentItem.Published)
                    {
                        //todo: this published false-positive draft events when user tries to publish and server side validation fails
                        eventType = "draft";
                    }
                    break;
            }

            if (eventType != null)
            {
                ContentEvent contentEvent = new ContentEvent(workflowContext.CorrelationId, contentItem, eventType);
                await _eventGridContentClient.Publish(contentEvent);
            }

            //todo: do we need to offload as before, as if event need retrying ui will block for long time?

            return Outcomes("Done");
        }

//         #pragma warning disable S3241
//         private async Task ProcessEventAfterContentItemQuiesces(WorkflowExecutionContext workflowContext, ContentItem eventContentItem)
//         {
//             try
//             {
//                 await Task.Delay(5000);
//
//                 // content page
//                 // state,          user action, validated,
//                 // new             save draft   y
//                 // new             save draft   n
//                 // new             publish      y
//                 // new             publish      n
//                 // draft           save draft   y
//                 // draft           save draft   n <- how to tell failed validation, invoke model state updater? some other way. could still publish false positive event, unnecessary, but they'll get good data
//                 // draft           publish      y
//                 // draft           publish      n <- published==false, will publish (false positive) updated draft. latest = true, todo: can we distinguish between successful saved draft
//                 // published       save draft   y
//                 // published       save draft   n <- !published, has published date: can we use that to ignore, or does something else match? yes published, save draft, y. will publish draft event and there is no draft, although consumers should handle that as they need to be able to handle a completely different state anyway, as there's a window between an published event and a consumer asking the api for it. so perhaps we have to just live with superfluous events when validation fails?
//                 // published       publish      y
//                 // published       publish      n <- current false posive draft event. can we use modified after publish date? no same as save draft from published
//                 // draft+published
//                 // draft+published
//                 // draft+published
//                 // draft+published
//
//                 // content item page
//
//                 // import
//
//                 bool published = eventContentItem.Published;
// //todo: published isn't set if item already exists, and user is publishing a new version
//                 if (await HasFailedValidation(eventContentItem, published))
//                     return;
// //todo: publishes updated-draft when save draft of a published version, as the modified is the publish time. either just publish draft/published, or try getting specific versions in GetAsync
//                 //bool created = eventContentItem.CreatedUtc == eventContentItem.ModifiedUtc;
//
//                 // would it be better to use the workflowid as the correlation id instead?
//                 // should be bother having created/updated?
// //                ContentEvent contentEvent = new ContentEvent(workflowContext.CorrelationId, eventContentItem, $"{(created?"created":"updated")}-{(published?"publish":"draft")}");
//                 ContentEvent contentEvent = new ContentEvent(workflowContext.CorrelationId, eventContentItem, $"new-{(published?"published":"draft")}");
//                 await _eventGridContentClient.Publish(contentEvent);
//             }
//             catch (Exception e)
//             {
//                 // as we fire and forget this method, any errors won't cause the workflow to fail, so we must make sure the log can be tied back to the workflow
//                 _logger.LogError($"Delayed processing of workflow id {workflowContext.WorkflowId} failed: {e}");
//             }
//         }
//         #pragma warning restore S3241
//
//         private async Task<bool> HasFailedValidation(ContentItem contentItem, bool published)
//         {
//             // if content item is new and validation failed, ContentItemVersionId = null, CreatedUtc is null, ModifiedUtc is set, latest = false, published = false
//             if (contentItem.ContentItemVersionId == null)
//                 return true;
//
//             try
//             {
//                 // if content item already existed and validation failed, the event content item has a later modified date than the quiesced content item
//                 ContentItem contentItemBeforeCurrentOperation = await _contentManager.GetAsync(contentItem.ContentItemId, published ? VersionOptions.Published : VersionOptions.Draft);
//
//                 #pragma warning disable S1481
//                 ContentItem publishedContentItemBeforeCurrentOperation = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Published);
//                 ContentItem draftContentItemBeforeCurrentOperation = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Draft);
//
//
//                 //todo: doesn't work for publish, then draft as existing content item has earlier modified date
//                 return contentItemBeforeCurrentOperation.ModifiedUtc < contentItem.ModifiedUtc;
//             }
//             catch //todo: if have draft & published, then publish, published is false, so ends up publishing new draft
//             {//todo: if have draft & published, and new pub or draft, it doesn't get the old version
//                 // validation succeeded but the item is new
//                 return false;
//             }
//         }
    }
}
