using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class WorkingLocationContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkingLocationContentItem(string title, string timestamp, string description, string contentItemId) : base("WorkingLocation", title, timestamp, description, contentItemId)
        {
        }
    }
}
