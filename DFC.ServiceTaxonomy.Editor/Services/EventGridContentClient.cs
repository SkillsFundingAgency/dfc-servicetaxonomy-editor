using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Models;
using Microsoft.Rest.Azure;

namespace DFC.ServiceTaxonomy.Editor.Services
{
    public interface IEventGridContentClient
    {
        Task<CloudError> Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default);
        Task<CloudError> Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default);
    }

    public class EventGridContentClient : IEventGridContentClient
    {
        private readonly IRestHttpClient _restHttpClient;

        public EventGridContentClient(IRestHttpClient restHttpClient)
        {
            _restHttpClient = restHttpClient;
        }

        public async Task<CloudError> Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default)
        {
            //todo: do you get a 200 and clouderror? do you always get a clouderror?
             return await Publish(new[] {contentEvent}, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CloudError> Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default)
        {
            //todo: do you get a 200 and clouderror? do you always get a clouderror?
            return await _restHttpClient.PostAsJson<IEnumerable<ContentEvent>, CloudError>("", contentEvents, cancellationToken).ConfigureAwait(false);
        }
    }
}
