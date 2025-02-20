using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Intefrace;
using DfE.NCS.Framework.Event.Model;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.CompUi.Handlers
{
    public class EventGridHandler : IEventGridHandler
    {
        public const string StaxCreateMessage = "/cache-refresh/stax/create/";
        public const string StaxUpdateMessage = "/cache-refresh/stax/update/";
        public const string StaxDeleteMessage = "/cache-refresh/stax/delete/";

        private readonly INcsEventGridClient _eventGridClient;
        private readonly ILogger<EventGridHandler> _logger;

        public EventGridHandler(INcsEventGridClient eventGridClient, ILogger<EventGridHandler> logger)
        {
            _eventGridClient = eventGridClient;
            _logger = logger;
        }

        public async Task SendEventMessageAsync(RelatedContentData contentData, ContentEventType contentEventType)
        {
            try
            {
                ContentEventData? eventData = CreateEventMessageAsync(contentData);

                if (eventData != null)
                {
                    switch (contentEventType)
                    {
                        case (ContentEventType.StaxCreate):
                            await _eventGridClient.Publish(new ContentEvent(eventData, ContentEventType.StaxCreate, StaxCreateMessage + eventData.ContentType));
                            break;
                        case (ContentEventType.StaxUpdate):
                            await _eventGridClient.Publish(new ContentEvent(eventData, ContentEventType.StaxUpdate, StaxUpdateMessage + eventData.ContentType));
                            break;
                        case (ContentEventType.StaxDelete):
                            await _eventGridClient.Publish(new ContentEvent(eventData, ContentEventType.StaxDelete, StaxDeleteMessage + eventData.ContentType));
                            break;
                    }
                }
                else
                {
                    _logger.LogError($"Current content could not be retrieved for Content Item ID: {contentData.ContentItemId}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Current content could not be retrieved from published item: {ex.Message}");
                throw;
            }
        }

        public ContentEventData? CreateEventMessageAsync(RelatedContentData contentData)
        {
            if (contentData != null)
            {
                ContentEventData eventData = new ContentEventData
                {
                    Author = contentData?.Author ?? string.Empty,
                    DisplayText = contentData?.DisplayText ?? string.Empty,
                    ContentItemId = contentData?.ContentItemId ?? string.Empty,
                    ContentType = contentData?.ContentType,
                    GraphSyncId = contentData?.GraphSyncId?.Substring(contentData.GraphSyncId.LastIndexOf('/') + 1) ?? string.Empty,
                    FullPageUrl = contentData?.FullPageUrl ?? string.Empty
                };

                return eventData;
            }
            else
            {
                return null;
            }
        }
    }
}
