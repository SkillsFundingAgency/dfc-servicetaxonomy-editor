namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class CollegeLinkContentItem : AcademicEntryRouteLinkContentItem
    {
        public CollegeLinkContentItem(string title, string sitefinityLink, string timestamp, string contentItemId)
            : base("CollegeLink", title, sitefinityLink, timestamp, contentItemId)
        {
        }
    }
}
