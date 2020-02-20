using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class OtherRequirementContentItem : TitleHtmlDescriptionContentItem
    {
        public OtherRequirementContentItem(string title, string timestamp, string description, string contentItemId) : base("OtherRequirement", title, timestamp, description, contentItemId)
        {
        }
    }
}
