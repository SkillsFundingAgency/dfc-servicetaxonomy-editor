using System.Diagnostics;

namespace DFC.ServiceTaxonomy.Events.Models
{
    public class ContentEventData
    {
        public string Api { get; }
        public string ItemId { get; }
        public string VersionId { get; }
        public string DisplayText { get; }
        public string Author { get; }
        public string TraceId { get; private set; }
        public string ParentId { get; private set; }

        public ContentEventData(string api, string itemId, string versionId, string displayText, string author, Activity activity)
        {
            Api = api;
            ItemId = itemId;
            VersionId = versionId;
            DisplayText = displayText;
            Author = author;
            TraceId = activity.TraceId.ToString();
            //The parent id for the receiving request
            ParentId = activity.SpanId.ToString();
        }
    }
}
