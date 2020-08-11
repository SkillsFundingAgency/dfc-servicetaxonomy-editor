using DFC.ServiceTaxonomy.Taxonomies.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.Taxonomies.ViewModels
{
    public class TaxonomyPartEditViewModel
    {
        public string Hierarchy { get; set; }

        public string TermContentType { get; set; }

        [BindNever]
        public TaxonomyPart TaxonomyPart { get; set; }
    }
}
