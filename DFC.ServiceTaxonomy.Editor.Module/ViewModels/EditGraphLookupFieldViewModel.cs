using System.Collections.Generic;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class EditGraphLookupFieldViewModel
    {
        public string? ItemIds { get; set; }

        public GraphLookupField? Field { get; set; }
        public ContentPart? Part { get; set; }
        public ContentPartFieldDefinition? PartFieldDefinition { get; set; }

        //todo: no need for list?
        [BindNever]
        public IList<VueMultiselectItemViewModel>? SelectedItems { get; set; }
    }

    public class VueMultiselectItemViewModel
    {
        public string? DisplayText { get; set; }
        public string? Value { get; set; }
    }
}
