
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class UniversityLinkContentItem : AcademicEntryRouteLinkContentItem
    {
        public UniversityLinkContentItem(string title, string sitefinityUrl, string timestamp, string contentItemId)
            : base("UniversityLink", title, sitefinityUrl, timestamp, contentItemId)
        {
        }
    }
}
