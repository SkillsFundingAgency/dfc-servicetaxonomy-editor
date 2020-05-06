using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Models;

namespace DFC.ServiceTaxonomy.Editor.Services
{
    public interface IRestHttpClientFactory
    {
        RestHttpClient CreateClient(string name);
    }

    public class RestHttpClientFactory : IRestHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RestHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public RestHttpClient CreateClient(string name)
        {
            return new RestHttpClient(_httpClientFactory.CreateClient(name));
        }
    }

    public interface IEventGridContentClient
    {
        Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default);
        Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default);
    }

    public class EventGridContentClient : IEventGridContentClient
    {
        private readonly IRestHttpClientFactory _restHttpClientFactory;

        public EventGridContentClient(IRestHttpClientFactory restHttpClientFactory)
        {
            _restHttpClientFactory = restHttpClientFactory;
        }

        public async Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default)
        {
            await GetRestHttpClient(contentEvent.ContentType).PostAsJson("", contentEvent, cancellationToken).ConfigureAwait(false);
            //await Publish(new[] {contentEvent}, cancellationToken).ConfigureAwait(false);
        }

        public async Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default)
        {
            // only required if we have a topic for each content type and we get called with ContentEvents with different ContentTypes
            //IEnumerable<string> distinctContentTypes = contentEvents.Select(e => e.ContentType).Distinct();

            var distinctEventGroups = contentEvents.GroupBy(e => e.ContentType, e => e);

            var postTasks = distinctEventGroups.Select(g =>
                GetRestHttpClient(g.Key).PostAsJson("", g, cancellationToken));

            await Task.WhenAll(postTasks);

            // var response = await _httpClient.PostAsJsonAsync("", contentEvents, cancellationToken).ConfigureAwait(false);
            //
            // if (response.IsSuccessStatusCode)
            //     return null;
            //
            // Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            // return await JsonSerializer.DeserializeAsync<CloudError>(contentStream, cancellationToken: cancellationToken)!;
        }

        private readonly Dictionary<string, IRestHttpClient> _contentTypeRestClients = new Dictionary<string, IRestHttpClient>();

        private IRestHttpClient GetRestHttpClient(string contentType)
        {
            if (_contentTypeRestClients.TryGetValue(contentType, out IRestHttpClient? restHttpClient))
                return restHttpClient;

            //todo: how does it handle missing?
            IRestHttpClient? newRestHttpClient = _restHttpClientFactory.CreateClient(contentType)
                ?? _restHttpClientFactory.CreateClient("*");

            //todo: new exception? MissingEventGridTopicConfigurationException(contentType)
            _contentTypeRestClients[contentType] = newRestHttpClient
                                                   ?? throw new ApplicationException("todo");

            return newRestHttpClient;
        }
    }
}
