using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Models
{
    public class BannerType : ContentPart
    {
        public string? Id { get; set; }
        public string? DisplayText { get; set; }
        public bool HasPublished { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsActive { get; set; }
    }
}
