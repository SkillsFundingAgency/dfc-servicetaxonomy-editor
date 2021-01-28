using DFC.ServiceTaxonomy.Taxonomies.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Taxonomies.ViewModels
{
    public class TaxonomyPartViewModel
    {
        public string TaxonomyContentItemId => ContentItem.ContentItemId;

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public TaxonomyPart TaxonomyPart { get; set; }
    }
}
