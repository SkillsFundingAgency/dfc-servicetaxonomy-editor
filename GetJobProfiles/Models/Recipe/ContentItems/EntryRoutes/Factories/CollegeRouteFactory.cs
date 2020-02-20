using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class CollegeRouteFactory : RouteFactory
    {
        public CollegeRouteContentItem CreateCollegeRoute(AcademicEntryRoute entryRoute, string title,
            string timestamp, string contentItemId = null)
        {
            var route = new CollegeRouteContentItem(entryRoute, title, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
