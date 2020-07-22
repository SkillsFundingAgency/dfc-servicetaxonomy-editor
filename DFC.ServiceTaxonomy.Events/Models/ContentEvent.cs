using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Events.Models
{
    public enum ContentEventType
    {
        Published,
        Unpublished,
        Draft,
        DraftDiscarded,
        Deleted
    }

    public class ContentEvent
    {
        // use 2 part segmented eventType?
        public ContentEvent(
            //IGraphSyncHelper graphSyncHelper,
            string correlationId,
            ContentItem contentItem,
            string userId,
            ContentEventType contentEventType)
        {
            Id = Guid.NewGuid().ToString();

            //todo: use GraphSyncHelper?
            // do we assume id ends with a guid, or do we need a setting to extract the eventgrid id from the full id?
            // string userId = contentItem.Content.GraphSyncPart.Text;

            //string userId = graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart);
            string itemId = userId.Substring(userId.Length - 36);
            Subject = $"/content/{contentItem.ContentType.ToLower()}/{itemId}";

            Data = new ContentEventData(userId, itemId, contentItem.ContentItemVersionId, contentItem.DisplayText, contentItem.Author, correlationId, Activity.Current);
            EventType = GetEventType(contentEventType);
            EventTime = (contentItem.ModifiedUtc ?? contentItem.CreatedUtc)!.Value;
            MetadataVersion = null;
            DataVersion = "1.0";
            ContentType = contentItem.ContentType;
        }

        public string Id { get; }
        public string Subject { get; }
        public ContentEventData Data { get; }
        public string EventType { get; }
        public DateTime EventTime { get; }
        public string? MetadataVersion { get; }
        public string DataVersion { get; }

        [JsonIgnore]
        public string ContentType { get; }

        public override string ToString()
        {
            return $"Id: {Id}, EventType: {EventType}, Subject: {Subject}";
        }

        private string GetEventType(ContentEventType contentEventType)
        {
            string eventType = contentEventType.ToString().ToLowerInvariant();
            return eventType == "draftdiscarded" ? "draft-discarded" : eventType;
        }
    }
}
