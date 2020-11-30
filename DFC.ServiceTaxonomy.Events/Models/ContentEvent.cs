﻿using System;
using System.Diagnostics;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Events.Models
{
    public class ContentEvent
    {
        // use 2 part segmented eventType?
        public ContentEvent(
            ContentItem contentItem,
            string userId,
            ContentEventType contentEventType)
        {
            Id = Guid.NewGuid().ToString();

            //todo: use SyncNameProvider?
            // do we assume id ends with a guid, or do we need a setting to extract the eventgrid id from the full id?
            // string userId = contentItem.Content.GraphSyncPart.Text;

            string itemId = userId.Substring(userId.Length - 36);
            Subject = $"/content/{contentItem.ContentType.ToLower()}/{itemId}";

            //todo: the new activity should be Stop()ed
            Data = new ContentEventData(userId, itemId, contentItem.ContentItemVersionId, contentItem.DisplayText, contentItem.Author, contentItem.ContentType, Activity.Current ?? new Activity(nameof(ContentEvent)).Start());
            EventType = GetEventType(contentEventType);
            EventTime = DateTime.UtcNow;
            MetadataVersion = null;
            DataVersion = "1.0";
        }

        public string Id { get; }
        public string Subject { get; }
        public ContentEventData Data { get; }
        public string EventType { get; }
        public DateTime EventTime { get; }
        public string? MetadataVersion { get; }
        public string DataVersion { get; }

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
