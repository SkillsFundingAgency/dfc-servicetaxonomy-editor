using OrchardCore.Entities;

namespace GetJobProfiles.Models.Recipe.ContentItems.Base
{
    public class ContentItem
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        public const int MaxDisplayTextLength = 250;
        private string _displayText;

        public ContentItem(string contentType, string title, string timestamp, string contentItemId = null)
        {
            ContentItemId = contentItemId ?? _generator.GenerateUniqueId(); //"[js:uuid()]";
            ContentItemVersionId = _generator.GenerateUniqueId(); //"[js:uuid()]";
            ContentType = contentType;
            DisplayText = title;
            Latest = true;
            Published = true;
            ModifiedUtc = timestamp;
            PublishedUtc = timestamp;
            CreatedUtc = timestamp;
            // these parameters not available during non-setup recipe by the looks of it
            // Owner = "[js: parameters('AdminUsername')]";
            // Author = "[js: parameters('AdminUsername')]";
            // these need to match what's been used in the envs
            Owner = "admin";
            Author = "admin";
        }

        public string ContentItemId { get; set; }
        public string ContentItemVersionId { get; set; }
        public string ContentType { get; set; }

        public string DisplayText
        {
            get => _displayText;
            set => _displayText = Truncate(value);
        }

        public bool Latest { get; set; }
        public bool Published { get; set; }
        public string ModifiedUtc { get; set; }
        public string PublishedUtc { get; set; }
        public string CreatedUtc { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }

        private string Truncate(string displayText)
        {
            // display text is stored in the oc database in a column that has a fixed size
            return displayText.Substring(0, ContentItem.MaxDisplayTextLength);
        }
    }
}
