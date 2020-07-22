using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Models
{
    public class PageLocationPart : ContentPart
    {
        public string? UrlName { get; set; }
        public bool DefaultPageAtLocation { get; set; }
        public string? RedirectLocations { get; set; }
        public string? FullUrl { get; set; }
    }
}
