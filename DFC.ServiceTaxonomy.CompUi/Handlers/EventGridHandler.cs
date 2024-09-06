using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Intefrace;
using DfE.NCS.Framework.Event.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        /// <summary>
        /// Sends Event grid messages to Event Grid Topic using data from the published content item
        /// </summary>
        /// <param name="processing">Published content item data</param>
        /// <param name="contentEventType">Publish event type</param>
        public async Task SendEventMessageAsync(Processing processing, ContentEventType contentEventType)
        {
            try
            {
                ContentEventData? eventData = CreateEventMessageAsync(processing);

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
                        default:
                            _logger.LogError($"Current content could not be retrieved for Content Item ID: {processing.ContentItemId}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Current content could not be retrieved from published item: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates the content event message using the published content item data
        /// </summary>
        /// <param name="processing">Published content item data</param>
        /// <returns>Returns the content event data to be sent as part of the event grid message</returns>
        public ContentEventData? CreateEventMessageAsync(Processing processing)
        {
                ContentItem? result;

                if (!string.IsNullOrEmpty(processing.CurrentContent))
                {
                    result = JsonConvert.DeserializeObject<ContentItem>(processing.CurrentContent);

                    ContentEventData eventData = new ContentEventData
                    {
                        Author = processing.Author,
                        DisplayText = processing.DisplayText,
                        ContentItemId = processing.ContentItemId,
                        ContentType = processing.ContentType,
                        GraphSyncId = result?.GraphSyncParts?.Text?.Substring(result.GraphSyncParts.Text.LastIndexOf('/') + 1)
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
