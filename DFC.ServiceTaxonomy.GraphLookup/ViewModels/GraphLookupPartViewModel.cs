using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;

namespace DFC.ServiceTaxonomy.GraphLookup.ViewModels
{
    public class GraphLookupPartViewModel
    {
        public string MySetting { get; set; }

        public bool Show { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public GraphLookupPart GraphLookupPart { get; set; }

        [BindNever]
        public GraphLookupPartSettings Settings { get; set; }
    }
}
