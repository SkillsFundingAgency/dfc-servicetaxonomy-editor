using DFC.ServiceTaxonomy.Banners.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Banners.ViewModels
{
    public class BannerPartViewModel
    {
        public string? WebPageName { get; set; } 
        public string? WebPageURL { get; set; }
        public BannerPart? BannerPart { get; set; }
        public ContentPart? Part { get; set; }
        public ContentTypePartDefinition? PartDefinition { get; set; }
    }
}
