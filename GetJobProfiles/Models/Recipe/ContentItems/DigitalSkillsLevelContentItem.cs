using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class DigitalSkillsLevelContentItem : ContentItem
    {
        public DigitalSkillsLevelContentItem(string title, string timestamp, string contentItemId = null)
             : base("DigitalSkillsLevel", title, timestamp, contentItemId = null)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("DigitalSkillLevel");
            DisplayText = TitlePart.Title;
        }

        public virtual DigitalSkillsLevelPart EponymousPart { get; set; }

        public TitlePart TitlePart { get; set; }

        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
