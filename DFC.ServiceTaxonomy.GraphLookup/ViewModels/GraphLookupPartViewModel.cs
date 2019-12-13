using DFC.ServiceTaxonomy.GraphLookup.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GraphLookupPartSettings = DFC.ServiceTaxonomy.GraphLookup.Settings.GraphLookupPartSettings;

namespace DFC.ServiceTaxonomy.GraphLookup.ViewModels
{
    public class GraphLookupPartViewModel
    {
        //todo: required??
        // public string? Value { get; set; }
        // public string? DisplayText { get; set; }

        public string? ItemIds { get; set; }

        // [BindNever]
        // public ContentItem? ContentItem { get; set; }

        [BindNever]
        public GraphLookupPart? GraphLookupPart { get; set; }

        //[BindNever]
        //public ContentPartFieldDefinition? PartFieldDefinition { get; set; }
        [BindNever]
        public string? PartName { get; set; }

        [BindNever]
        public GraphLookupPartSettings? Settings { get; set; }

        [BindNever]
        public GraphLookupNode[]? SelectedItems { get; set; }
    }
}
