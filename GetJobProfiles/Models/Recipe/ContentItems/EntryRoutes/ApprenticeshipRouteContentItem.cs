using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class ApprenticeshipRouteContentItem : AcademicEntryRouteContentItem
    {
        public ApprenticeshipRouteContentItem(string title, AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
            : base("ApprenticeshipRoute", title, entryRoute, timestamp, contentItemId)
        {
        }
    }
}
