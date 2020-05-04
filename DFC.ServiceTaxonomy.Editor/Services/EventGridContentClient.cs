using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Models;

namespace DFC.ServiceTaxonomy.Editor.Services
{
    public interface IEventGridContentClient
    {
        Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default);
        Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default);
    }

    public class EventGridContentClient : IEventGridContentClient
    {
        private readonly IRestHttpClient _restHttpClient;

        public EventGridContentClient(IRestHttpClient restHttpClient)
        {
            _restHttpClient = restHttpClient;
        }

        // private readonly HttpClient _httpClient;
        // public EventGridContentClient(HttpClient httpClient)
        // {
        //     _httpClient = httpClient;
        // }

        public async Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default)
        {
             await Publish(new[] {contentEvent}, cancellationToken).ConfigureAwait(false);
        }

        public async Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default)
        {
            //todo: do you get a 200 and clouderror? do you always get a clouderror?
            await _restHttpClient.PostAsJson("", contentEvents, cancellationToken).ConfigureAwait(false);

            // var response = await _httpClient.PostAsJsonAsync("", contentEvents, cancellationToken).ConfigureAwait(false);
            //
            // if (response.IsSuccessStatusCode)
            //     return null;
            //
            // Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            // return await JsonSerializer.DeserializeAsync<CloudError>(contentStream, cancellationToken: cancellationToken)!;
        }
    }
}
