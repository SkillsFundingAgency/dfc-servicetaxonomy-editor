using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class CollegeLinkContentItem : AcademicEntryRouteLinkContentItem
    {
        public CollegeLinkContentItem(string title, string sitefinityLink, string timestamp, string contentItemId)
            : base("CollegeLink", title, sitefinityLink, timestamp, contentItemId)
        {
        }
    }
}
