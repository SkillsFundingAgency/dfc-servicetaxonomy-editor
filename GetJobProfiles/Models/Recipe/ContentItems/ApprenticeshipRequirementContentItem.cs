namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class ApprenticeshipRequirementContentItem : TitleHtmlDescriptionContentItem
    {
        public ApprenticeshipRequirementContentItem(string title, string timestamp, string description, string contentItemId) : base("ApprenticeshipRequirement", title, timestamp, description, contentItemId)
        {
        }
    }
}
