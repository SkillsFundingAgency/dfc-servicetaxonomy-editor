using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.GraphLookup.ViewModels
{
    public class GraphLookupPartViewModel
    {
        public string? ItemIds { get; set; }

        [BindNever]
        public GraphLookupPart? GraphLookupPart { get; set; }

        [BindNever]
        public string? PartName { get; set; }

        [BindNever]
        public GraphLookupPartSettings? Settings { get; set; }

        [BindNever]
        public GraphLookupNode[]? SelectedItems { get; set; }
    }
}
