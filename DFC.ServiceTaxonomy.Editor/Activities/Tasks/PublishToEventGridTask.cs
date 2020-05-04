using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Models;
using DFC.ServiceTaxonomy.Editor.Services;
using Microsoft.Extensions.Localization;
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
        //        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridOptionsMonitor;

        public PublishToEventGridTask(
//            IOptionsMonitor<EventGridConfiguration> EventGridOptionsMonitor,
            IEventGridContentClient eventGridContentClient,
            IStringLocalizer<PublishToEventGridTask> localizer)
        {
            _eventGridContentClient = eventGridContentClient;
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
            //todo: inject RestHttpClient

            //todo: support >1 topic
            // EventGridConfiguration eventGridConfig = _eventGridOptionsMonitor.CurrentValue;
            //
            // EventGridTopicConfiguration topicConfig = eventGridConfig.Endpoints.First();
            //
            // //todo: cache RestHttpClient's by contenttype??
            // //todo: check config for null and throw meaningful exceptions
            //
            // var httpClient = new HttpClient
            // {
            //     BaseAddress = new Uri(topicConfig.TopicEndpoint!),
            //     DefaultRequestHeaders =
            //     {
            //         {"aeg-sas-key", topicConfig.AegSasKey}
            //     }
            // };
            //
            // var client = new RestHttpClient(httpClient);

            ContentItem contentItem = (ContentItem)workflowContext.Input["ContentItem"];

            //todo: use GraphSyncHelper
            // do we assume id ends with a guid, or do we need a setting to extract the eventgrid id from the full id?
            // string userId = contentItem.Content.GraphSyncPart.Text;
            //
            // EventGridClient
            // //todo: EventGridEvent is using newtonsoft.json JsonProperty
            // EventGridEvent contentEvent = new EventGridEvent(
            //     workflowContext.CorrelationId,
            //     $"/content/{contentItem.ContentType}/{userId.Substring(userId.Length-36)}",
            //     );
            //
            // await client.PostAsJson("", contentEvent);

            //todo: how to get status? do we need multiple instances of this task?? (with common base?)
            ContentEvent contentEvent = new ContentEvent(workflowContext.CorrelationId, contentItem, "published");

            //todo: follow SyncToGraphTask, or do we return Failed outcome and add the notification to the workflow?

            // try
            // {
                await _eventGridContentClient.Publish(contentEvent);
                return Outcomes("Done");
            // }
            // catch (Exception e)
            // {
            //     throw;
            // }
        }
    }
}
