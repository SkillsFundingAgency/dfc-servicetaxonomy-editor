using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class DayToDayTaskContentItem : TitleHtmlDescriptionContentItem
    {
        public DayToDayTaskContentItem(string title, string timestamp, string description, string contentItemId) : base("DayToDayTask", title, timestamp, description, contentItemId)
        {
        }
    }
}
