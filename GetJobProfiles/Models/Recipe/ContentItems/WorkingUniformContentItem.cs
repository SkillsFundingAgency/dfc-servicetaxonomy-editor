using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class WorkingUniformContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkingUniformContentItem(string title, string timestamp, string description, string contentItemId) : base("WorkingUniform", title, timestamp, description, contentItemId)
        {
        }
    }
}
