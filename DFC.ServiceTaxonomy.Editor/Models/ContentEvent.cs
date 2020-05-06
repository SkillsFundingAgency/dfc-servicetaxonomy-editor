using System;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Editor.Models
{
    public class ContentEvent
    {
        public ContentEvent(
            string correlationId,
            ContentItem contentItem,
            // string id,
            // string subject,
            // object data,
            //todo: pass in 2
            string eventType)
            // DateTime eventTime,
            // string dataVersion,
            // string topic = null,
            // string metadataVersion = null)
        {
            Id = correlationId;
            // Topic = topic;

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
        // public string Topic { get; set; }
        public string Subject { get; }
        public ContentEventData Data { get; }
        public string EventType { get; }
        public DateTime EventTime { get; }
        public string? MetadataVersion { get; }
        public string DataVersion { get; }

        [JsonIgnore]
        public string ContentType { get; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        // public virtual void Validate()
        // {
        //     if (Id == null)
        //     {
        //         throw new ValidationException(ValidationRules.CannotBeNull, "Id");
        //     }
        //     if (Subject == null)
        //     {
        //         throw new ValidationException(ValidationRules.CannotBeNull, "Subject");
        //     }
        //     if (Data == null)
        //     {
        //         throw new ValidationException(ValidationRules.CannotBeNull, "Data");
        //     }
        //     if (EventType == null)
        //     {
        //         throw new ValidationException(ValidationRules.CannotBeNull, "EventType");
        //     }
        //     if (DataVersion == null)
        //     {
        //         throw new ValidationException(ValidationRules.CannotBeNull, "DataVersion");
        //     }
        // }
    }
}
