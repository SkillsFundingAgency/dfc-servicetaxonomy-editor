using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Models;

namespace DFC.ServiceTaxonomy.Events.Services
{
    public interface IEventGridContentRestHttpClientFactory
    {
        IRestHttpClient CreateClient(string contentType);
    }

    public class EventGridContentRestHttpClientFactory : IEventGridContentRestHttpClientFactory
    {
        private static readonly ConcurrentDictionary<string, IRestHttpClient> _contentTypeRestClients = new ConcurrentDictionary<string, IRestHttpClient>();
        private readonly IHttpClientFactory _httpClientFactory;

        public EventGridContentRestHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IRestHttpClient CreateClient(string contentType)
        {
            if (_contentTypeRestClients.TryGetValue(contentType, out IRestHttpClient? restHttpClient))
                return restHttpClient;

            HttpClient httpClient = _httpClientFactory.CreateClient(contentType);
            if (httpClient.BaseAddress == null)
            {
                httpClient = _httpClientFactory.CreateClient("*");

                //todo: check * exists in startup and/or here?
                if (httpClient.BaseAddress == null)
                    throw new MissingEventGridTopicConfigurationException(contentType);
            }

            restHttpClient = new RestHttpClient(httpClient);
            _contentTypeRestClients[contentType] = restHttpClient;

            return restHttpClient;
        }
    }

    public interface IEventGridContentClient
    {
        Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default);
        Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default);
    }

    public class EventGridContentClient : IEventGridContentClient
    {
        private readonly IEventGridContentRestHttpClientFactory _eventGridContentRestHttpClientFactory;

        public EventGridContentClient(IEventGridContentRestHttpClientFactory eventGridContentRestHttpClientFactory)
        {
            _eventGridContentRestHttpClientFactory = eventGridContentRestHttpClientFactory;
        }

        public async Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default)
        {
            await _eventGridContentRestHttpClientFactory.CreateClient(contentEvent.ContentType)
                .PostAsJson("", contentEvent, cancellationToken);
        }

        public async Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default)
        {
            // only required if we have a topic for each content type and we get called with ContentEvents with different ContentTypes
            //IEnumerable<string> distinctContentTypes = contentEvents.Select(e => e.ContentType).Distinct();

            var distinctEventGroups = contentEvents.GroupBy(e => e.ContentType, e => e);

            var postTasks = distinctEventGroups.Select(g =>
                _eventGridContentRestHttpClientFactory.CreateClient(g.Key).PostAsJson("", g, cancellationToken));

            await Task.WhenAll(postTasks);
        }
    }
}
