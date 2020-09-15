using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class PersonalityTraitContentItem : ContentItem
    {
        public PersonalityTraitContentItem(string title, string timestamp, string contentItemId = null)
          : base("PersonalityTrait", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("PersonalTrait");
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public PersonalityTraitPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
