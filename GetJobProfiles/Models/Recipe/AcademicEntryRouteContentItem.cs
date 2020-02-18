namespace GetJobProfiles.Models.Recipe
{
    public class UniversityRouteContentItem : AcademicEntryRouteContentItem
    {
        public UniversityRouteContentItem(string title, string timestamp, string contentItemId = null)
            : base("UniversityRoute", title, timestamp, contentItemId)
        {
        }
    }

    public class CollegeRouteContentItem : AcademicEntryRouteContentItem
    {
        public CollegeRouteContentItem(string title, string timestamp, string contentItemId = null)
            : base("CollegeRoute", title, timestamp, contentItemId)
        {
        }
    }

    public class ApprenticeshipRouteContentItem : AcademicEntryRouteContentItem
    {
        public ApprenticeshipRouteContentItem(string title, string timestamp, string contentItemId = null)
            : base("ApprenticeshipRoute", title, timestamp, contentItemId)
        {
        }
    }

    public class AcademicEntryRouteContentItem : ContentItem
    {
        public AcademicEntryRouteContentItem(string contentType, string title, string timestamp, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
        }
    }
}
