namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class RelatedContentData
    {
        public string? ContentItemId { get; set; }
        public string? ContentType { get; set; }
        public string GraphSyncId { get; set; }
        public string DisplayText { get; set; }
        public string Author { get; set; }
        public string FullPageUrl { get; set; }
    }

    public class RelatedContentDataList
    {
        public List<RelatedContentData> ItemList { get; set; }
    }
}
