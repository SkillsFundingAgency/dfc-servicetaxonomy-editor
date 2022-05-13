using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class ApprenticeshipStandardContentItem : ContentItem
    {
        public ApprenticeshipStandardContentItem(string title, string timestamp, string contentItemId = null)
            : base("ApprenticeshipStandard", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("ApprenticeshipStandard");
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public ApprenticeshipStandardPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
