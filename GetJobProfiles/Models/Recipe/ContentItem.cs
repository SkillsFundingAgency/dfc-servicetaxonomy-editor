namespace GetJobProfiles.Models.Recipe
{
    public class ContentItem
    {
        public string ContentItemId { get; set; }
        public string ContentItemVersionId { get; set; }
        public string ContentType { get; set; }
        public string DisplayText { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
        public string ModifiedUtc { get; set; }
        public string PublishedUtc { get; set; }
        public string CreatedUtc { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }
        public TitlePart TitlePart { get; set; }
    }

    public class TitlePart
    {
        public string Title { get; set; }
    }
}
