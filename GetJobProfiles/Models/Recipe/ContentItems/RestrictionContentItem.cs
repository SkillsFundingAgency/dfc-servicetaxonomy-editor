using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class RestrictionContentItem : TitleHtmlDescriptionContentItem
    {
        public RestrictionContentItem(string title, string timestamp, string description, string contentItemId) : base("Restriction", title, timestamp, description, contentItemId)
        {
        }
    }
}
