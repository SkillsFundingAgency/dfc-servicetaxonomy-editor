using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class DigitalSkillsLevelContentItem : TitleHtmlDescriptionContentItem
    {
        public DigitalSkillsLevelContentItem(string title, string timestamp, string description) : base("DigitalSkillsLevel", title, timestamp, description)
        {
        }
    }
}
