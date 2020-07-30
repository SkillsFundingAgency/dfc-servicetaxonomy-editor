using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.Events.Handlers
{
    public class PublishToEventGridHandler : ContentHandlerBase
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly ILogger<PublishToEventGridHandler> _logger;

        public PublishToEventGridHandler(IOptionsMonitor<EventGridConfiguration> eventGridConfiguration, IEventGridContentClient eventGridContentClient, IGraphSyncHelper graphSyncHelper, IPublishedContentItemVersion publishedContentItemVersion, IPreviewContentItemVersion previewContentItemVersion, ILogger<PublishToEventGridHandler> logger)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
            _graphSyncHelper = graphSyncHelper;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _logger = logger;
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside DraftSavedAsync");
            await PublishContentEvent(context.ContentItem, _previewContentItemVersion, ContentEventType.Draft);
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside PublishedAsync");
            await PublishContentEvent(context.ContentItem, _publishedContentItemVersion, ContentEventType.Published);

            if (context.ContentItem.CreatedUtc != context.ContentItem.ModifiedUtc)
            {
                await PublishContentEvent(context.ContentItem, _previewContentItemVersion, ContentEventType.DraftDiscarded);
            }
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside UnpublishedAsync");
            await PublishContentEvent(context.ContentItem, _publishedContentItemVersion, ContentEventType.Unpublished);
            await PublishContentEvent(context.ContentItem, _previewContentItemVersion, ContentEventType.Draft);
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside RemovedAsync");

            if (context.NoActiveVersionLeft)
            {
                await PublishContentEvent(context.ContentItem, _publishedContentItemVersion, ContentEventType.Deleted);
                await PublishContentEvent(context.ContentItem, _previewContentItemVersion, ContentEventType.Deleted);
            }
            else
            {
                // discard draft
                await PublishContentEvent(context.ContentItem, _previewContentItemVersion, ContentEventType.DraftDiscarded);
            }
        }

        private async Task PublishContentEvent(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            ContentEventType eventType)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside PublishContentEvent");

            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return;
            }

            _logger.LogInformation($"PublishToEventGridHandler::Publishing event: {eventType}");

            string userId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            ContentEvent contentEvent = new ContentEvent(contentItem, userId, eventType);
            await _eventGridContentClient.Publish(contentEvent);
        }
    }
}
