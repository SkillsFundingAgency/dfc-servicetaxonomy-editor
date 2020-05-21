using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class ApprenticeshipStandardRouteContentItem : ContentItem
    {
        public ApprenticeshipStandardRouteContentItem(string title, string timestamp, string contentItemId = null)
            : base("ApprenticeshipStandardRoute", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("ApprenticeshipStandardRoute");
            DisplayText = TitlePart.Title;
            ContentItemId = contentItemId;
        }

        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
