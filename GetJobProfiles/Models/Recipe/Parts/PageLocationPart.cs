using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class PageLocationPart
    {
        public string? UrlName { get; set; }
        public string? FullUrl { get; set; }
        public bool DefaultPageForLocation { get; set; }
        public string? RedirectLocations { get; set; }
    }
}
