using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.Events.Activities.Tasks
{
    //todo: revisit if/when we get ContentSavedEvent
    //todo: remove false-positives
    //todo: publish draft when a new draft is cloned

    /// <summary>
    /// | existing state      | user action              | server      | post state         | event grid events          | notes                                   |
    /// |                     |                          | validation  |                    |                            |                                         |
    /// |                     |                          |:passed     :|                    |                            |                                         |
    /// |---------------------|--------------------------|-------------|--------------------|----------------------------|-----------------------------------------|
    /// | n/a                 | save draft               |     y       | draft              | draft                      |                                         |
    /// | n/a                 | save draft               |     n       | n/a                | n/a                        |                                         |
    /// | n/a>draft val fail  | save draft               |     y       | draft              | draft                      |                                         |
    /// | n/a>draft val fail  | save draft               |     n       | n/a                | n/a                        |                                         |
    /// | n/a>pub val fail    | save draft               |     y       | draft              | draft                      |                                         |
    /// | n/a>pub val fail    | save draft               |     n       | n/a                | n/a                        |                                         |
    /// | n/a                 | publish                  |     y       | published          | published                  |                                         |
    /// | n/a                 | publish                  |     n       | n/a                | n/a                        |                                         |
    /// | n/a>draft val fail  | publish                  |     y       | published          | published                  |                                         |
    /// | n/a>draft val fail  | publish                  |     n       | n/a                | n/a                        |                                         |
    /// | n/a>pub val fail    | publish                  |     y       | published          | published                  |                                         |
    /// | n/a>pub val fail    | publish                  |     n       | n/a                | n/a                        |                                         |
    /// | draft               | save draft               |     y       | draft              | draft                      |                                         |
    /// | draft               | save draft               |     n       | draft              | draft                      | * false positive                        |
    /// | draft               | publish                  |     y       | published          | published+draft-discarded  |                                         |
    /// | draft               | publish                  |     n       | draft              | draft                      | * false positive                        |
    /// | draft               | publish draft from list  |             | published          | published                  |                                         |
    /// | published           | save draft               |     y       | draft+published    | draft                      |                                         |
    /// | published           | save draft               |     n       | published          | draft                      | * false positive (no draft exists)      |
    /// | published           | publish                  |     y       | published          | published                  |                                         |
    /// | published           | publish                  |     n       | published          | draft                      | * false positive (no draft exists)      |
    /// | published           | publish draft from list  |             | published          | n/a                        | publishing without changes is a no-op   |
    /// | draft+published     | save draft               |     y       | draft+published    | draft                      |                                         |
    /// | draft+published     | save draft               |     n       | draft+published    | draft                      | * false positive                        |
    /// | draft+published     | publish                  |     y       | published          | published+draft-discarded  |                                         |
    /// | draft+published     | publish                  |     n       | draft+published    | draft                      | * false positive                        |
    /// | draft+published     | publish from list        |             | published          | published+draft-discarded  |                                         |
    /// | published           | unpublish from list      |             | draft              | unpublished+draft          |                                         |
    /// | draft+published     | unpublish from list      |             | draft              | unpublished                | draft is unchanged                      |
    /// | draft+published     | discard draft from list  |             | published          | draft-discarded            |                                         |
    /// | draft               | delete from list         |             | n/a                | deleted                    |                                         |
    /// | published           | delete from list         |             | n/a                | deleted                    |                                         |
    /// | draft+published     | delete from list         |             | n/a                | deleted                    |                                         |
    /// | draft               | clone from list          |             | new draft          | n/a                        | * currently no way to know that a (draft) clone has been created |
    /// | published           | clone from list          |             | new draft          | n/a                        | * currently no way to know that a (draft) clone has been created |
    /// | draft+published     | clone from list          |             | new draft          | n/a                        | * currently no way to know that a (draft) clone has been created |
    /// | n/a                 | import recipe (publish)  |             | published          | published                  | re-importing not tested as it has issues that should be fixed by the current oc idempotent recipes pr |
    /// </summary>
    public class PublishToEventGridTask : TaskActivity
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly IContentManager _contentManager;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly ILogger<PublishToEventGridTask> _logger;

        public PublishToEventGridTask(
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            IContentManager contentManager,
            IGraphSyncHelper graphSyncHelper,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IStringLocalizer<PublishToEventGridTask> localizer,
            ILogger<PublishToEventGridTask> logger)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
            _contentManager = contentManager;
            _graphSyncHelper = graphSyncHelper;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
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

            var preDelayDraftContentItem =
                await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Draft);
            var preDelayPublishedContentItem =
                await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Published);

#pragma warning disable CS4014
            // defer processing, so that we can view the state of the contentitem when the system quiesces,
            // and if we end up retrying the calls to event grid, we don't freeze the ui
            ProcessEventAfterContentItemQuiesces(workflowContext, contentItem, preDelayDraftContentItem, preDelayPublishedContentItem);
#pragma warning restore CS4014

            return Outcomes("Done");
        }

        /// <remarks>
        /// We can't use any scoped services in here (and neither does creating a new scope work for anything using a session),
        /// we can only use transients and singletons.
        /// </remarks>
        #pragma warning disable S3241 // not a good idea to return void from an async
        private async Task ProcessEventAfterContentItemQuiesces(
            WorkflowExecutionContext workflowContext,
            ContentItem eventContentItem,
            ContentItem preDelayDraftContentItem,
            ContentItem preDelayPublishedContentItem)
        #pragma warning restore S3241
        {
            try
            {
                await Task.Delay(5000);

                // new item failed server side validation
                if (eventContentItem.ContentItemVersionId == null)
                    return;

                ContentEventType? eventType = null;
                ContentEventType? eventType2 = null;

                string trigger = (string)workflowContext.Properties["Trigger"];
                switch (trigger)
                {
                    case "published":
                        eventType = ContentEventType.Published;
                        if (preDelayPublishedContentItem.CreatedUtc != preDelayPublishedContentItem.ModifiedUtc)
                        {
                            eventType2 = ContentEventType.DraftDiscarded;
                        }
                        break;
                    case "updated":
                        if (!eventContentItem.Published
                            && (preDelayPublishedContentItem != null
                                //todo: don't think this is required anymore??
                                || (preDelayDraftContentItem?.ModifiedUtc == null ||
                                    eventContentItem.ModifiedUtc >= preDelayDraftContentItem.ModifiedUtc)))
                        {
                            eventType = ContentEventType.Draft;
                        }
                        break;
                    case "unpublished":
                        eventType = ContentEventType.Unpublished;
                        if (preDelayPublishedContentItem.ModifiedUtc == eventContentItem.ModifiedUtc)
                        {
                            eventType2 = ContentEventType.Draft;
                        }

                        break;
                    case "deleted":
                        if (preDelayPublishedContentItem?.Published == true)
                        {
                            // discard draft
                            eventType = ContentEventType.DraftDiscarded;
                        }
                        else
                        {
                            eventType = ContentEventType.Deleted;
                        }
                        break;
                }

                //todo: modified time as event time is probably wrong. either pick correct time, or just set to now
                if (eventType != null)
                {
                    await PublishContentEvents(workflowContext, eventContentItem, eventType.Value);
                }

                if (eventType2 != null)
                {
                    await PublishContentEvents(workflowContext, eventContentItem, eventType2.Value);
                }
            }
            catch (Exception e)
            {
                // as we fire and forget this method, any errors won't cause the workflow to fail, so we must make sure the log can be tied back to the workflow
                _logger.LogError($"Delayed processing of workflow id {workflowContext.WorkflowId} failed: {e}");
            }
        }

        private async Task PublishContentEvents(
            WorkflowExecutionContext workflowContext,
            ContentItem contentItem,
            ContentEventType eventType)
        {
            switch (eventType)
            {
                case ContentEventType.Published:
                case ContentEventType.Unpublished:
                    await PublishContentEvent(workflowContext, contentItem, _publishedContentItemVersion, eventType);
                    //todo: if there wasn't a draft version, would have to publish unpublished with pewview graph too
                    break;
                case ContentEventType.Draft:
                case ContentEventType.DraftDiscarded:
                    await PublishContentEvent(workflowContext, contentItem, _previewContentItemVersion, eventType);
                    break;
                case ContentEventType.Deleted:
                    //todo: should only publish events if there was a published/preview version
                    await PublishContentEvent(workflowContext, contentItem, _publishedContentItemVersion, eventType);
                    await PublishContentEvent(workflowContext, contentItem, _previewContentItemVersion, eventType);
                    break;
            }
        }

        private async Task PublishContentEvent(
            WorkflowExecutionContext workflowContext,
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            ContentEventType eventType)
        {
            // would it be better to use the workflowid as the correlation id instead?
            // ContentEvent contentEvent = ActivatorUtilities.CreateInstance<ContentEvent>(
            //     _serviceProvider, workflowContext.CorrelationId, contentItem, eventType);

            string userId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            ContentEvent contentEvent = new ContentEvent(workflowContext.CorrelationId, contentItem, userId, eventType);
            await _eventGridContentClient.Publish(contentEvent);
        }
    }
}
