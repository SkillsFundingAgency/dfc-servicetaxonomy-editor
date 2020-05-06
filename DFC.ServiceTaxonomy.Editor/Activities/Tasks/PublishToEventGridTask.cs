using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Models;
using DFC.ServiceTaxonomy.Editor.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.Editor.Activities.Tasks
{
    // using EventGridClient vs HttpRestClient
    // EventGridClient has lots of extras
    // e.g. sets x-ms-client-request-id as new guid (can't supply own) - what exactly is it? looks network related as opposed to correlation id
    // topicHostname gets passed to PublishEventsAsync. presumably that means needs to open a socket connection each time an event is published
    // and lose out on kept alive connections and the goodness you get using IHttpClientFactory
    // see, https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
    // EventGridClient and EventGridEvent use old newtonsoft.json, rather than System.Net.Http
    // EventGridViewer: get updates for free
    // https://github.com/Azure/azure-sdk-for-net/tree/fef6a5436167758454a9eb965ed1d7b3f8eb061b/sdk/eventgrid/Microsoft.Azure.EventGrid

    //todo use CloudError


    //todo: in own module?
    public class PublishToEventGridTask : TaskActivity
    {
        private readonly IEventGridContentClient _eventGridContentClient;

        private readonly IContentManager _contentManager;

        private readonly ILogger<PublishToEventGridTask> _logger;
        //        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridOptionsMonitor;

        public PublishToEventGridTask(
//            IOptionsMonitor<EventGridConfiguration> EventGridOptionsMonitor,
            IEventGridContentClient eventGridContentClient,
            IContentManager contentManager,
            IStringLocalizer<PublishToEventGridTask> localizer,
            ILogger<PublishToEventGridTask> logger)
        {
            _eventGridContentClient = eventGridContentClient;
            _contentManager = contentManager;
            _logger = logger;
            //          _eventGridOptionsMonitor = EventGridOptionsMonitor;
            T = localizer;
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Failed"]);
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(PublishToEventGridTask);
        public override LocalizedString DisplayText => T["Publish content state changes to Azure Event Grid"];
        public override LocalizedString Category => T["Event Grid"];

        public override async Task<ActivityExecutionResult> ExecuteAsync(
            WorkflowExecutionContext workflowContext,
            ActivityContext activityContext)
        {
            //todo: support >1 topic
            // //todo: cache RestHttpClient's by contenttype??
            // //todo: check config for null and throw meaningful exceptions

            ContentItem contentItem = (ContentItem)workflowContext.Input["ContentItem"];

            //Task.Delay(1000).ContinueWith(async t=> await DelayedEventProcessing(contentItem));

            if ((string)workflowContext.Properties["Trigger"] == "updated")
            {
                await ProcessEventAfterContentItemQuiesces(workflowContext.CorrelationId, contentItem);
            }

            //todo: use GraphSyncHelper
            // do we assume id ends with a guid, or do we need a setting to extract the eventgrid id from the full id?
            // string userId = contentItem.Content.GraphSyncPart.Text;

            //todo: add author to event??
            //todo: for retry probably use polly, could do through workflow, but probably too involved

            //todo: follow SyncToGraphTask, or do we return Failed outcome and add the notification to the workflow? we're gonna have to just return success, because of the delay
            // actually it doesn't matter if the workflow takes a while

            return Outcomes("Done");
        }

        private async Task ProcessEventAfterContentItemQuiesces(string workflowCorrelationId, ContentItem eventContentItem)
        {
            await Task.Delay(5000);

            // content page
            // state,          user action, validated,
            // new             save draft   y
            // new             save draft   n
            // new             publish      y
            // new             publish      n
            // draft           save draft   y
            // draft           save draft   n <- how to tell failed validation, invoke model state updater? some other way. could still publish false positive event, unnecessary, but they'll get good data
            // draft           publish      y
            // draft           publish      n <- published==false, will publish (false positive) updated draft. latest = true, todo: can we distinguish between successful saved draft
            // published       save draft   y
            // published       save draft   n <- !published, has published date: can we use that to ignore, or does something else match? yes published, save draft, y. will publish draft event and there is no draft, although consumers should handle that as they need to be able to handle a completely different state anyway, as there's a window between an published event and a consumer asking the api for it. so perhaps we have to just live with superfluous events when validation fails?
            // published       publish      y
            // published       publish      n <- current false posive draft event. can we use modified after publish date? no same as save draft from published
            // draft+published
            // draft+published
            // draft+published
            // draft+published

            // content item page

            // import

            // try getting contentitem from contentmanager (should be different if validation failed) will it be updated if validation passed?

            //todo: what about error / exception handling?

            if (await HasFailedValidation(eventContentItem))
                return;

            bool created = eventContentItem.CreatedUtc == eventContentItem.ModifiedUtc;

            //IsPublished/HasDraft - are there 2 separate contentitems, 1 published and 1 draft

            bool published = eventContentItem.Published;

            // would it be better to use the workflowid as the correlation id instead?
            // should be bother having created/updated?
            ContentEvent contentEvent = new ContentEvent(workflowCorrelationId, eventContentItem, $"{(created?"created":"updated")}-{(published?"publish":"draft")}");
            await _eventGridContentClient.Publish(contentEvent);
        }

        private async Task<bool> HasFailedValidation(ContentItem contentItem)
        {
            // if content item is new and validation failed, ContentItemVersionId = null, CreatedUtc is null, ModifiedUtc is set, latest = false, published = false
            if (contentItem.ContentItemVersionId == null)
                return true;

            try
            {
                // if content item already existed and validation failed, then the event content item has a later modified date than the quiesced content item
                ContentItem quiescedContentItem = await _contentManager.GetAsync(contentItem.ContentItemId);
                return quiescedContentItem.ModifiedUtc < contentItem.ModifiedUtc;
            }
            catch
            {
                //todo: can't remember when this occurs - add comment
                return false;
            }
        }
    }
}
