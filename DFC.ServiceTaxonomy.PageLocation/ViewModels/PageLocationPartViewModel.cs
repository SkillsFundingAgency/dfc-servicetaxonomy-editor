using DFC.ServiceTaxonomy.PageLocation.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.ViewModels
{
    public class PageLocationPartViewModel
    {
        public string? UrlName { get; set; }
        public bool DefaultPageForLocation { get; set; }
        public string? RedirectLocations { get; set; }
        public string? FullUrl { get; set; }

        [BindNever]
        public ContentItem? ContentItem { get; set; }

        [BindNever]
        public PageLocationPart? PageLocationPart { get; set; }

        [BindNever]
        public PageLocationPartSettings? Settings { get; set; }
    }
}
