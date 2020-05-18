namespace DFC.ServiceTaxonomy.Events.Models
{
    public class ContentEventData
    {
        public string Api { get; }
        public string ItemId { get; }
        public string VersionId { get; }
        public string DisplayText { get; }
        public string Author { get; }

        public ContentEventData(string api, string itemId, string versionId, string displayText, string author)
        {
            Api = api;
            ItemId = itemId;
            VersionId = versionId;
            DisplayText = displayText;
            Author = author;
        }
    }
}
