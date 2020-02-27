using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class JobCategoryContentItem : ContentItem
    {
        public JobCategoryContentItem(string title, string timestamp)
            : base("JobCategory", title, timestamp)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("JobCategory");
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public JobCategoryPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
