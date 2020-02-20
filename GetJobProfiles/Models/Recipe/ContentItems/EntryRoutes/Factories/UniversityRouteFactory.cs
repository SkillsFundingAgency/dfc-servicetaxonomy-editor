using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class UniversityRouteFactory : RouteFactory
    {
        public UniversityRouteContentItem CreateUniversityRoute(AcademicEntryRoute entryRoute, string title,
            string timestamp, string contentItemId = null)
        {
            var route = new UniversityRouteContentItem(entryRoute, title, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
