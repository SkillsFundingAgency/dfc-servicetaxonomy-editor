using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Events.Services
{
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
