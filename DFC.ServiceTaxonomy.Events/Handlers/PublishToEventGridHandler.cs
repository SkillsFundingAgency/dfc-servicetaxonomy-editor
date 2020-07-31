using System;
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

        private static readonly DeletedContentItemVersion _deletedContentItemVersion = new DeletedContentItemVersion();

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
            await PublishContentEvent(context.ContentItem, ContentEventType.Draft);
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside PublishingAsync");
            await PublishContentEvent(context.ContentItem, ContentEventType.Published);
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside UnpublishedAsync");
            await PublishContentEvent(context.ContentItem, ContentEventType.Unpublished);
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside RemovedAsync");

            if (context.NoActiveVersionLeft)
            {
                //TODO : refactor to new way where only a single event is fired
                await PublishContentEvent(context.ContentItem, ContentEventType.Deleted);
            }
            else
            {
                // discard draft
                await PublishContentEvent(context.ContentItem, ContentEventType.DraftDiscarded);
            }
        }

        private async Task PublishContentEvent(
            ContentItem contentItem,
            ContentEventType eventType)
        {
            _logger.LogInformation("PublishToEventGridHandler::Inside PublishContentEvent");

            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return;
            }

            _logger.LogInformation($"PublishToEventGridHandler::Publishing event: {eventType}");

            IContentItemVersion contentItemVersion = eventType switch
            {
                ContentEventType.Published => _publishedContentItemVersion,
                ContentEventType.Unpublished => _publishedContentItemVersion,
                ContentEventType.Draft => _previewContentItemVersion,
                ContentEventType.DraftDiscarded => _previewContentItemVersion,
                _ => _deletedContentItemVersion
            };

            string userId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            ContentEvent contentEvent = new ContentEvent(contentItem, userId, eventType);
            await _eventGridContentClient.Publish(contentEvent);
        }

        private class DeletedContentItemVersion : IContentItemVersion
        {
            public string ContentApiBaseUrl => "";

            public VersionOptions VersionOptions => throw new NotImplementedException();
            public (bool? latest, bool? published) ContentItemIndexFilterTerms => throw new NotImplementedException();
            public string GraphReplicaSetName => throw new NotImplementedException();
            public Task<ContentItem> GetContentItem(IContentManager contentManager, string id) => throw new NotImplementedException();
        }
    }
}
