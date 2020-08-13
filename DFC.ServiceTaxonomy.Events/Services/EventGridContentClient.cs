using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Events.Services
{
    public class EventGridContentClient : IEventGridContentClient
    {
        private readonly IEventGridContentRestHttpClientFactory _eventGridContentRestHttpClientFactory;
        private readonly ILogger<EventGridContentClient> _logger;

        public EventGridContentClient(
            IEventGridContentRestHttpClientFactory eventGridContentRestHttpClientFactory,
            ILogger<EventGridContentClient> logger)
        {
            _eventGridContentRestHttpClientFactory = eventGridContentRestHttpClientFactory;
            _logger = logger;
        }

        public async Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default)
        {
            //todo: log topic
            _logger.LogInformation($"Publishing single event {contentEvent}");

            await _eventGridContentRestHttpClientFactory.CreateClient(contentEvent.Data.ContentType)
                .PostAsJson("", new[] {contentEvent}, cancellationToken);
        }

        public async Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default)
        {
            // only required if we have a topic for each content type and we get called with ContentEvents with different ContentTypes
            //IEnumerable<string> distinctContentTypes = contentEvents.Select(e => e.ContentType).Distinct();

            var distinctEventGroups = contentEvents.GroupBy(e => e.Data.ContentType, e => e);

            //todo: add logging

            var postTasks = distinctEventGroups.Select(g =>
                _eventGridContentRestHttpClientFactory.CreateClient(g.Key).PostAsJson("", g, cancellationToken));

            await Task.WhenAll(postTasks);
        }
    }
}
