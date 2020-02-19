namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class ApprenticeshipLinkContentItem : AcademicEntryRouteLinkContentItem
    {
        public ApprenticeshipLinkContentItem(string title, string sitefinityLink, string timestamp, string contentItemId)
            : base("ApprenticeshipLink", title, sitefinityLink, timestamp, contentItemId)
        {
        }
    }
}
