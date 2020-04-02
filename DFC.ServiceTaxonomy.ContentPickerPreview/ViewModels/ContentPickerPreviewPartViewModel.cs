using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels
{
    public class ContentPickerPreviewPartViewModel
    {
        // public string[] ContentItemIds { get; set; } = Array.Empty<string>();
        //
        // // [BindNever]
        // // public ContentItem ContentItem { get; set; }
        // //
        // // [BindNever]
        // // public ContentPickerPreviewPart ContentPickerPreviewPart { get; set; }
        //
        // [BindNever]
        // public ContentPickerPreviewPartSettings? Settings { get; set; }

        public string? ContentItemIds { get; set; }
        //public ContentPickerField Field { get; set; }
        //public ContentPart Part { get; set; }
        public ContentPickerPreviewPart? Part { get; set; }
        //public ContentPartFieldDefinition PartFieldDefinition { get; set; }
        public ContentPartDefinition? PartDefinition { get; set; }

        [BindNever]
        public IList<VueMultiselectItemViewModel>? SelectedItems { get; set; }

        [BindNever]
        public ContentPickerPreviewPartSettings? Settings { get; set; }
    }
}
