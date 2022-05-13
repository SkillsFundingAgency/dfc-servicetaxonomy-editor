using GetJobProfiles.Models.API;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class UniversityRouteFactory : RouteFactory
    {
        public UniversityRouteContentItem Create(string title, AcademicEntryRoute entryRoute,
            string timestamp, string contentItemId = null)
        {
            var route = new UniversityRouteContentItem(title, entryRoute, timestamp, contentItemId);
            AddContentPickers(entryRoute, route);
            return route;
        }
    }
}
