using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class CollegeRouteFactory : RouteFactory
    {
        public CollegeRouteContentItem CreateCollegeRoute(AcademicEntryRoute entryRoute,
            string timestamp, string contentItemId = null)
        {
            var route = new CollegeRouteContentItem(entryRoute, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
