using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class UniversityRequirementContentItem : TitleHtmlDescriptionContentItem
    {
        public UniversityRequirementContentItem(string title, string timestamp, string description, string contentItemId) : base("UniversityRequirement", title, timestamp, description, contentItemId)
        {
        }
    }
}
