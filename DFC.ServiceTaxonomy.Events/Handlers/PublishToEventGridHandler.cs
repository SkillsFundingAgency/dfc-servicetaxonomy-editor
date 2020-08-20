using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.Events.Handlers
{
    //todo: update comment and confluence with which content uri prefix is used

    /// <summary>
    /// |Id| existing state      | user action              | post state         | event grid events          | notes                                   |
    /// |--|---------------------|--------------------------|--------------------|----------------------------|-----------------------------------------|
    /// |1 | n/a                 | save draft               | draft              | draft                      |                                         |
    /// |2 | n/a>draft val fail  | save draft               | draft              | draft                      |                                         |
    /// |3 | n/a>pub val fail    | save draft               | draft              | draft                      |                                         |
    /// |4 | n/a                 | publish                  | published          | published                  |                                         |
    /// |5 | n/a>draft val fail  | publish                  | published          | published                  |                                         |
    /// |6 | n/a>pub val fail    | publish                  | published          | published                  |                                         |
    /// |7 | draft               | save draft               | draft              | draft                      |                                         |
    /// |8 | draft               | publish                  | published          | published                  |                                         |
    /// |9 | draft               | publish draft from list  | published          | published                  |                                         |
    /// |10| published           | save draft               | draft+published    | draft                      |                                         |
    /// |11| published           | publish                  | published          | published                  |                                         |
    /// |12| published           | publish draft from list  | published          | n/a                        | publishing without changes is a no-op   |
    /// |13| draft+published     | save draft               | draft+published    | draft                      |                                         |
    /// |14| draft+published     | publish                  | published          | published                  |                                         |
    /// |15| draft+published     | publish from list        | published          | published                  |                                         |
    /// |16| published           | unpublish from list      | draft              | unpublished+draft          |                                         |
    /// |17| draft+published     | unpublish from list      | draft              | unpublished                | draft is unchanged                      |
    /// |18| draft+published     | discard draft from list  | published          | draft-discarded            |                                         |
    /// |19| draft               | delete from list         | n/a                | deleted                    |                                         |
    /// |20| published           | delete from list         | n/a                | deleted                    |                                         |
    /// |21| draft+published     | delete from list         | n/a                | deleted                    |                                         |
    /// |22| draft               | clone from list          | new draft          | draft                      |                                         |
    /// |23| published           | clone from list          | new draft          | draft                      |                                         |
    /// |24| draft+published     | clone from list          | new draft          | draft                      |                                         |
    /// |25| n/a                 | import recipe (publish)  | published          | published                  | re-importing not tested as it has issues that should be fixed by the current oc idempotent recipes pr |
    /// </summary>
    public class PublishToEventGridHandler : ContentHandlerBase
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly INeutralContentItemVersion _neutralContentItemVersion;
        private readonly ILogger<PublishToEventGridHandler> _logger;

        public PublishToEventGridHandler(
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            IGraphSyncHelper graphSyncHelper,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            INeutralContentItemVersion neutralContentItemVersion,
            ILogger<PublishToEventGridHandler> logger)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
            _graphSyncHelper = graphSyncHelper;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _neutralContentItemVersion = neutralContentItemVersion;
            _logger = logger;
        }

        public override async Task ClonedAsync(CloneContentContext context)
        {
            await PublishContentEvent(context.CloneContentItem, ContentEventType.Draft);
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            await PublishContentEvent(context.ContentItem, ContentEventType.Draft);
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            await PublishContentEvent(context.ContentItem, ContentEventType.Published);
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            await PublishContentEvent(context.ContentItem, ContentEventType.Unpublished);
            await PublishContentEvent(context.ContentItem, ContentEventType.Draft);
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            if (context.NoActiveVersionLeft)
            {
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
            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return;
            }

            IContentItemVersion contentItemVersion = eventType switch
            {
                ContentEventType.Published => _publishedContentItemVersion,
                ContentEventType.Draft => _previewContentItemVersion,
                _ => _neutralContentItemVersion
            };

            string userId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            ContentEvent contentEvent = new ContentEvent(contentItem, userId, eventType);
            await _eventGridContentClient.Publish(contentEvent);
        }
    }
}
