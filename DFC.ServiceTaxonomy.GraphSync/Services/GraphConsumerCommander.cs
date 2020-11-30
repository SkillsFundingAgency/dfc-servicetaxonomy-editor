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

            const string commandHandlerSubjectPrefix = "/graph/commands/handler/";

            var allEventGridSubscriptions = await _restHttpClient.GetUsingNewtonsoft<Page<EventSubscription>>("");

            //todo: if multiple advanced filters, they are ANDed together, so how do we handle that?

            _graphConsumers = allEventGridSubscriptions
                .Where(s => IsHandler(commandHandlerSubjectPrefix, s.Filter.SubjectBeginsWith)
                                                                  //todo: values
                                                                  || s.Filter.AdvancedFilters.OfType<StringBeginsWithAdvancedFilter>().Any(af =>
                                                                      af.Key == "subject"
                                                                      && af.Values.Any(v => IsHandler(commandHandlerSubjectPrefix, v))))
                .Select(s => new GraphConsumer(s.Name))
                .ToList();

            //}

            return _graphConsumers;
        }

        //todo: unit test
        private bool IsHandler(string commandHandlerSubjectPrefix, string filterValue)
        {
            if (commandHandlerSubjectPrefix.Length > filterValue.Length)
                return commandHandlerSubjectPrefix.StartsWith(filterValue);

            return filterValue.StartsWith(commandHandlerSubjectPrefix);
        }

        public void InformConsumersGraphGoingDown()
        {
            throw new NotImplementedException();
        }
    }
}
