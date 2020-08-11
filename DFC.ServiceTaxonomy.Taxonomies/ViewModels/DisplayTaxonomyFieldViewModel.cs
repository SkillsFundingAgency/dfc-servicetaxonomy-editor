using DFC.ServiceTaxonomy.Taxonomies.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Taxonomies.ViewModels
{
    public class DisplayTaxonomyFieldViewModel
    {
        public string TaxonomyContentItemId => Field.TaxonomyContentItemId;
        public string[] TermContentItemIds => Field.TermContentItemIds;
        public TaxonomyField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
