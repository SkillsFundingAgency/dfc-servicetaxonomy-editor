using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.ViewModels;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels
{
    public class EditContentPickerPreviewPartViewModel
    {
        public string? ContentItemIds { get; set; }
        //public ContentPickerField Field { get; set; }
        //public ContentPart Part { get; set; }
        public ContentPickerPreviewPart? Part { get; set; }
        //public ContentPartFieldDefinition PartFieldDefinition { get; set; }

        [BindNever]
        public IList<VueMultiselectItemViewModel>? SelectedItems { get; set; }
    }
}
