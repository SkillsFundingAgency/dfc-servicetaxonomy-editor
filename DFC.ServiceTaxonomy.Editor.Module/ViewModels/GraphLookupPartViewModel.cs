using System.Collections.Generic;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{//oc internally throws coz not implementing IShape, but oc own parts don't either
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

        //todo: no need for list?
        [BindNever]
        public IList<VueMultiselectItemViewModel>? SelectedItems { get; set; }
    }
}
