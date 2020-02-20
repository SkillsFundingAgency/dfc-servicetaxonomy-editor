using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems.Base
{
    public class TitleTextDescriptionContentItem : ContentItem
    {
        public TitleTextDescriptionContentItem(string contentType, string title, string timestamp, string description, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            EponymousPart = new TitleTextDescriptionPart
            {
                Description = new TextField(description)
            };

            // update DisplayText with transformed title
            //todo: transform DisplayText and use that to initialize title?
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public TitleTextDescriptionPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class TitleTextDescriptionPart
    {
        public TextField Description { get; set; }
    }
}
