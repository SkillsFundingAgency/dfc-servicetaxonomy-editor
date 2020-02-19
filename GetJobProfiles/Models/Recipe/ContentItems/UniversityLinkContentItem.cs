using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class UniversityLinkContentItem : ContentItem
    {
        public UniversityLinkContentItem(string title, string timestamp, string contentItemId) : base("UniversityLink", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("UniversityLink");
        }

        public LinkField Link { get; set; }
        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
