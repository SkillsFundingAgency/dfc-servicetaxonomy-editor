using System;
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
    //todo: delete/unpublish
    public class PublishToEventGridTask : TaskActivity
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly ILogger<PublishToEventGridTask> _logger;

        public PublishToEventGridTask(
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            IStringLocalizer<PublishToEventGridTask> localizer,
            ILogger<PublishToEventGridTask> logger)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
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

        public override Task<ActivityExecutionResult> ExecuteAsync(
            WorkflowExecutionContext workflowContext,
            ActivityContext activityContext)
        {
            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return Task.FromResult(Outcomes("Done"));
            }

            ContentItem contentItem = (ContentItem)workflowContext.Input["ContentItem"];

            // defer processing, so that if we end up retrying the calls to event grid, we don't freeze the ui
#pragma warning disable CS4014
            ProcessEventAfterContentItemQuiesces(workflowContext, contentItem);
#pragma warning restore CS4014

            return Task.FromResult(Outcomes("Done"));
        }

        #pragma warning disable S3241
        private async Task ProcessEventAfterContentItemQuiesces(WorkflowExecutionContext workflowContext, ContentItem eventContentItem)
        {
            try
            {
                await Task.Delay(5000);

                // new item failed server side validation
                if (eventContentItem.ContentItemVersionId == null)
                    return;

                string? eventType = null;

                string trigger = (string)workflowContext.Properties["Trigger"];
                switch (trigger)
                {
                    case "published":
                        //todo: when a draft item is published, the draft version goes away. either we publish an event to say draft-removed
                        // or the consumer can remove drafts on a published event
                        eventType = "published";
                        break;
                    case "updated":
                        if (!eventContentItem.Published)
                        {
                            //todo: this publishes false-positive draft events when user tries to publish/draft an existing item and server side validation fails
                            //todo: this publishes false-positive draft events when user publishes a draft item
                            eventType = "draft";
                        }
                        break;
                    case "unpublished":
                        eventType = "unpublished";
                        break;
                    case "deleted":
                        eventType = "deleted";
                        break;
                }

                if (eventType != null)
                {
                    // would it be better to use the workflowid as the correlation id instead?
                    ContentEvent contentEvent = new ContentEvent(workflowContext.CorrelationId, eventContentItem, eventType);
                    await _eventGridContentClient.Publish(contentEvent);
                }
            }
            catch (Exception e)
            {
                // as we fire and forget this method, any errors won't cause the workflow to fail, so we must make sure the log can be tied back to the workflow
                _logger.LogError($"Delayed processing of workflow id {workflowContext.WorkflowId} failed: {e}");
            }
        }
        #pragma warning restore S3241
    }
}
