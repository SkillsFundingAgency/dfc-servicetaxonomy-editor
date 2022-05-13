using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems.Base
{
    public class TitleHtmlDescriptionContentItem : ContentItem
    {
        public TitleHtmlDescriptionContentItem(string contentType, string title, string timestamp, string description, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            EponymousPart = new TitleHtmlDescriptionPart
            {
                Description = new HtmlField(description)
            };

            DisplayText = TitlePart.Title;
        }

        public TitleHtmlDescriptionContentItem(string contentType, string title, string timestamp, IEnumerable<string> description,
            string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            EponymousPart = new TitleHtmlDescriptionPart
            {
                Description = new HtmlField(description)
            };

            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public virtual TitleHtmlDescriptionPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class TitleHtmlDescriptionPart
    {
        public HtmlField Description { get; set; }
    }
}
