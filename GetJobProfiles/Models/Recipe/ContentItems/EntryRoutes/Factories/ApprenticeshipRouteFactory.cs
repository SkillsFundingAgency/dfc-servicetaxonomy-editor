using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class ApprenticeshipRouteFactory : RouteFactory
    {
        public ApprenticeshipRouteContentItem Create(string title, AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
        {
            var route = new ApprenticeshipRouteContentItem(title, entryRoute, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
