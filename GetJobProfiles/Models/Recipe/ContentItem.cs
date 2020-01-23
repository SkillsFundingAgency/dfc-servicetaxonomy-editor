namespace GetJobProfiles.Models.Recipe
{
    public class ContentItem
    {
        public string ContentItemId;
        public string ContentItemVersionId;
        public string ContentType;
        public string DisplayText;
        public bool Latest;
        public bool Published;
        public string ModifiedUtc;
        public string PublishedUtc;
        public string CreatedUtc;
        public string Owner;
        public string Author;
        public TitlePart TitlePart;
    }

    public class TitlePart
    {
        public string Title;
    }
}
