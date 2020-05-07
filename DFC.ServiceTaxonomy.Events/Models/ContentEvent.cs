using System;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Events.Models
{
    public class ContentEvent
    {
        // use 2 part segmented eventType?
        public ContentEvent(string correlationId, ContentItem contentItem, string eventType)
        {
            Id = correlationId;

            string userId = contentItem.Content.GraphSyncPart.Text;
            Subject = $"/content/{contentItem.ContentType}/{userId.Substring(userId.Length - 36)}";

            Data = new ContentEventData(userId, contentItem.ContentItemVersionId, contentItem.DisplayText);
            EventType = eventType;
            //todo: can modified be null?
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
    }
}
