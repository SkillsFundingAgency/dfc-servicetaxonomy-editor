using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class RequirementsPrefixContentItem : TitleHtmlDescriptionContentItem
    {
        public RequirementsPrefixContentItem(string title, string timestamp, string description, string contentItemId)
            : base("RequirementsPrefix", title, timestamp, description, contentItemId)
        {
        }
    }
}
