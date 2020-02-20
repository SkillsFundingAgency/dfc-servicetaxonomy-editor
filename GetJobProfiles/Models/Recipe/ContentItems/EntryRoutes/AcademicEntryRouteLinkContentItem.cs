using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class AcademicEntryRouteLinkContentItem : ContentItem
    {
        public AcademicEntryRouteLinkContentItem(string contentType, string title, string sitefinityUrl, string timestamp, string contentItemId)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            DisplayText = TitlePart.Title;

            GraphSyncPart = new GraphSyncPart(contentType);

            Link = new LinkField(sitefinityUrl);
        }

        public LinkField Link { get; set; }
        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
