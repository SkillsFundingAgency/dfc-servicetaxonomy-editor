using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    //todo: this probably belongs in a module/project that sits on top of graphsync, as it's nothing to do with graph syncing
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
    public class EventGridPublishingHandler : IContentOrchestrationHandler
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly INeutralEventContentItemVersion _neutralEventContentItemVersion;
        private readonly ILogger<EventGridPublishingHandler> _logger;

        public EventGridPublishingHandler(
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            ISyncNameProvider syncNameProvider,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            INeutralEventContentItemVersion neutralEventContentItemVersion,
            ILogger<EventGridPublishingHandler> logger)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
            _syncNameProvider = syncNameProvider;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _neutralEventContentItemVersion = neutralEventContentItemVersion;
            _logger = logger;
        }

        public Task DraftSaved(IOrchestrationContext context)
        {
            return PublishContentEvent(context, ContentEventType.Draft);
        }

        public Task Published(IOrchestrationContext context)
        {
            return PublishContentEvent(context, ContentEventType.Published);
        }

        public async Task Unpublished(IOrchestrationContext context)
        {
            await PublishContentEvent(context, ContentEventType.Unpublished);
            await PublishContentEvent(context, ContentEventType.Draft);
        }

        public Task Cloned(IOrchestrationContext context)
        {
            return PublishContentEvent(context, ContentEventType.Draft);
        }

        public Task Deleted(IOrchestrationContext context)
        {
            return PublishContentEvent(context, ContentEventType.Deleted);
        }

        public Task DraftDiscarded(IOrchestrationContext context)
        {
            return PublishContentEvent(context, ContentEventType.DraftDiscarded);
        }

        #pragma warning disable S4144
        public async Task Restored(IOrchestrationContext context)
        {
            await PublishContentEvent(context, ContentEventType.Unpublished);
            await PublishContentEvent(context, ContentEventType.Draft);
        }
        #pragma warning restore S4144

        private async Task PublishContentEvent(
            IOrchestrationContext context,
            ContentEventType eventType)
        {
            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return;
            }

            try
            {
                IContentItemVersion contentItemVersion = eventType switch
                {
                    ContentEventType.Published => _publishedContentItemVersion,
                    ContentEventType.Draft => _previewContentItemVersion,
                    _ => _neutralEventContentItemVersion
                };

                string userId = _syncNameProvider.GetEventIdPropertyValue(
                    context.ContentItem.Content.GraphSyncPart,
                    contentItemVersion);

                ContentEvent contentEvent = new ContentEvent(context.ContentItem, userId, eventType);
                await _eventGridContentClient.Publish(contentEvent);
            }
            catch (Exception publishException)
            {
                _logger.LogError(publishException, "The event grid event could not be published.");
                await context.Notifier.Add("Warning: the event grid event could not be published. Composite apps might not show your changes.",
                    "Exception", publishException, type: NotifyType.Warning);
            }
        }
    }
}
