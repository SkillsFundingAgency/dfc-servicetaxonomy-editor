using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class PageLocationPart
    {
        public TextField? UrlName { get; set; }
        public BooleanField? DefaultPageForLocation { get; set; }
        public TextField? RedirectLocations { get; set; } = null;
        public TextField? FullUrl { get; set; }
    }
}
