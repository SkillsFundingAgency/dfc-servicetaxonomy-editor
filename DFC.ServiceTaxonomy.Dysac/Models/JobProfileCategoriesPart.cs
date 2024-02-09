using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Dysac.Models
{
    public class JobProfileCategoriesPart : ContentPart
    {
        public string? ContentItemId { get; set; }
        public string? RelatedJobProfileContentItemIds { get; set; }
    }
}
