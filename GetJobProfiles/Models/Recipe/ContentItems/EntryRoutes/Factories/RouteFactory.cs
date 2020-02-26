using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;
using GetJobProfiles.Models.Recipe.Fields.Factories;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories
{
    public class RouteFactory
    {
        protected void AddContentPickers(AcademicEntryRoute entryRoute, AcademicEntryRouteContentItem contentItem)
        {
            contentItem.EponymousPart.RequirementsPrefix = RequirementsPrefixes.CreateContentPicker(entryRoute.EntryRequirementPreface);
            contentItem.EponymousPart.Requirements = Requirements.CreateContentPicker(entryRoute.EntryRequirements);
            contentItem.EponymousPart.Links = Links.CreateContentPicker(entryRoute.AdditionalInformation);
        }

        // there are some route specific prefixes (e.g. To do this apprenticeship, you'll need:), but generally
        // there's a small shared set of prefixes, so we don't want route specific sets
        public static ContentPickerFactory RequirementsPrefixes { get; set; } = new ContentPickerFactory();
        public ContentPickerFactory Requirements { get; set; } = new ContentPickerFactory();
        public ContentPickerFactory Links { get; set; } = new ContentPickerFactory();
    }
}
