using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class UniversityRouteFactory : RouteFactory
    {
        public UniversityRouteContentItem CreateUniversityRoute(AcademicEntryRoute entryRoute,
            string timestamp, string contentItemId = null)
        {
            var route = new UniversityRouteContentItem(entryRoute, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
