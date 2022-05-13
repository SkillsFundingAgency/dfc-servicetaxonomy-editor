using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class CollegeRouteFactory : RouteFactory
    {
        public CollegeRouteContentItem Create(string title, AcademicEntryRoute entryRoute,
            string timestamp, string contentItemId = null)
        {
            var route = new CollegeRouteContentItem(title, entryRoute, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
