using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class UniversityRouteContentItem : AcademicEntryRouteContentItem
    {
        public UniversityRouteContentItem(string title, AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
            : base("UniversityRoute", title, entryRoute, timestamp, contentItemId)
        {
        }
    }
}
