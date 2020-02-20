using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class UniversityRouteContentItem : AcademicEntryRouteContentItem
    {
        public UniversityRouteContentItem(AcademicEntryRoute entryRoute, string title, string timestamp, string contentItemId = null)
            : base("UniversityRoute", entryRoute, title, timestamp, contentItemId)
        {
        }
    }
}
