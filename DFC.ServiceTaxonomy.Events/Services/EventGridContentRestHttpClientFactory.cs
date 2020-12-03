using System.Collections.Concurrent;
using System.Net.Http;
using DFC.ServiceTaxonomy.Events.Services.Exceptions;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.Services.Rest.Interfaces;

namespace DFC.ServiceTaxonomy.Events.Services
{
    public class EventGridContentRestHttpClientFactory : IEventGridContentRestHttpClientFactory
    {
        private static readonly ConcurrentDictionary<string, IRestHttpClient> _contentTypeRestClients = new();
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
}
