using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class WorkingEnvironmentContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkingEnvironmentContentItem(string title, string timestamp, string description, string contentItemId) : base("WorkingEnvironment", title, timestamp, description, contentItemId)
        {
        }
    }
}
