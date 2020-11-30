#pragma warning disable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Services;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public enum GraphConsumerStatus
    {
        InProgress,
        Success,
        Failed
    }

    public class GraphConsumer
    {
        public GraphConsumer(string name) => Name = name;

        // endpointBaseUrl
        public string? Name { get; set; }
    }

    public class GraphConsumerCommand
    {
        public string Id { get; set; }
        public GraphConsumerCommand()
        {
            Id = Guid.NewGuid().ToString("D");
        }

        public GraphConsumerStatus Status { get; set; }
    }

    public class GraphConsumerCommander : IGraphConsumerCommander
    {
        public const string EventGridSubscriptionsHttpClientName = "EventGridSubscriptions";

        private List<GraphConsumer>? _graphConsumers;
        private readonly RestHttpClient _restHttpClient;

        public GraphConsumerCommander(
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<EventGridSubscriptionsConfiguration> eventGridSubscriptionsConfiguration)
        {
            //todo: not picking up the named instance...
            var httpClient = httpClientFactory.CreateClient(EventGridSubscriptionsHttpClientName);
            // so need to do this
            httpClient.BaseAddress = new Uri(eventGridSubscriptionsConfiguration.CurrentValue.SubscriptionsApiUrl!);
            _restHttpClient = new RestHttpClient(httpClient);
            _graphConsumers = null;
        }

        public async Task<IEnumerable<GraphConsumer>> GetGraphConsumers()
        {
            //if (_graphConsumers == null)
            //{

            var allEventGridSubscriptions = await _restHttpClient.GetUsingNewtonsoft<Page<EventSubscription>>("");

            _graphConsumers = allEventGridSubscriptions
                .Where(s => s.Filter.SubjectBeginsWith.StartsWith("")
                //todo: values
                || s.Filter.AdvancedFilters.Any(af => af.Key == "subject"))
                .Select(s => new GraphConsumer(s.Name))
                .ToList();

            //}

            return _graphConsumers;
        }

        public void InformConsumersGraphGoingDown()
        {
            throw new NotImplementedException();
        }
    }
}
