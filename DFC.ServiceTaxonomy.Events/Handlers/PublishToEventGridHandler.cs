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
    //todo: revisit if/when we get ContentSavedEvent
    //todo: update comment and confluence with extra deleted events and which content uri prefix is used	

    /// <summary>	
    /// |Id| existing state      | user action              | post state         | event grid events          | notes                                   |	
    /// |  |                     |                          |                    |                            |                                         |	
    /// |  |                     |                          |                    |                            |                                         |	
    /// |--|---------------------|--------------------------|--------------------|----------------------------|-----------------------------------------|	
    /// |1 | n/a                 | save draft               | draft              | draft                      |                                         |	
    /// |2 | n/a                 | save draft               | n/a                | n/a                        |                                         |	
    /// |3 | n/a>draft val fail  | save draft               | draft              | draft                      |                                         |	
    /// |4 | n/a>draft val fail  | save draft               | n/a                | n/a                        |                                         |	
    /// |5 | n/a>pub val fail    | save draft               | draft              | draft                      |                                         |	
    /// |6 | n/a>pub val fail    | save draft               | n/a                | n/a                        |                                         |	
    /// |7 | n/a                 | publish                  | published          | published                  |                                         |	
    /// |8 | n/a                 | publish                  | n/a                | n/a                        |                                         |	
    /// |9 | n/a>draft val fail  | publish                  | published          | published                  |                                         |	
    /// |10| n/a>draft val fail  | publish                  | n/a                | n/a                        |                                         |	
    /// |11| n/a>pub val fail    | publish                  | published          | published                  |                                         |	
    /// |12| n/a>pub val fail    | publish                  | n/a                | n/a                        |                                         |	
    /// |13| draft               | save draft               | draft              | draft                      |                                         |	
    /// |14| draft               | save draft               | draft              | draft                      |                                         |	
    /// |15| draft               | publish                  | published          | published                  |                                         |	
    /// |16| draft               | publish                  | draft              | draft                      |                                         |	
    /// |17| draft               | publish draft from list  | published          | published                  |                                         |	
    /// |18| published           | save draft               | draft+published    | draft                      |                                         |	
    /// |19| published           | save draft               | published          | draft                      |                                         |	
    /// |20| published           | publish                  | published          | published                  |                                         |	
    /// |21| published           | publish                  | published          | draft                      |                                         |	
    /// |22| published           | publish draft from list  | published          | n/a                        | publishing without changes is a no-op   |	
    /// |23| draft+published     | save draft               | draft+published    | draft                      |                                         |	
    /// |24| draft+published     | save draft               | draft+published    | draft                      |                                         |	
    /// |25| draft+published     | publish                  | published          | published                  |                                         |	
    /// |26| draft+published     | publish                  | draft+published    | draft                      |                                         |	
    /// |27| draft+published     | publish from list        | published          | published                  |                                         |	
    /// |28| published           | unpublish from list      | draft              | unpublished                |                                         |	
    /// |29| draft+published     | unpublish from list      | draft              | unpublished                | draft is unchanged                      |	
    /// |30| draft+published     | discard draft from list  | published          | draft-discarded            |                                         |	
    /// |31| draft               | delete from list         | n/a                | deleted                    |                                         |	
    /// |32| published           | delete from list         | n/a                | deleted                    |                                         |	
    /// |33| draft+published     | delete from list         | n/a                | deleted                    |                                         |	
    /// |34| draft               | clone from list          | new draft          | draft                      |                                         |	
    /// |35| published           | clone from list          | new draft          | draft                      |                                         |	
    /// |36| draft+published     | clone from list          | new draft          | draft                      |                                         |	
    /// |37| n/a                 | import recipe (publish)  | published          | published                  | re-importing not tested as it has issues that should be fixed by the current oc idempotent recipes pr |	
    /// </summary>
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
