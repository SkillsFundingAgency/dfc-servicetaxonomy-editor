using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class RegistrationContentItem : TitleHtmlDescriptionContentItem
    {
        public RegistrationContentItem(string title, string timestamp, string description, string contentItemId) : base("Registration", title, timestamp, description, contentItemId)
        {
        }
    }
}
