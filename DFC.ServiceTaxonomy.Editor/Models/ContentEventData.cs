namespace DFC.ServiceTaxonomy.Editor.Models
{
    public class ContentEventData
    {
        public string Api { get; }
        public string VersionId { get; }
        public string DisplayText { get; }

        public ContentEventData(string api, string versionId, string displayText)
        {
            Api = api;
            VersionId = versionId;
            DisplayText = displayText;
        }
    }
}
