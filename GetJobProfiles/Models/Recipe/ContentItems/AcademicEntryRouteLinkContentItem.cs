using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class AcademicEntryRouteLinkContentItem : ContentItem
    {
        public AcademicEntryRouteLinkContentItem(string contentType, string title, string timestamp, string contentItemId) : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
        }

        public LinkField Link { get; set; }
        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
