using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class ApprenticeshipLinkContentItem : AcademicEntryRouteLinkContentItem
    {
        public ApprenticeshipLinkContentItem(string title, string sitefinityLink, string timestamp, string contentItemId)
            : base("ApprenticeshipLink", title, sitefinityLink, timestamp, contentItemId)
        {
        }
    }
}
