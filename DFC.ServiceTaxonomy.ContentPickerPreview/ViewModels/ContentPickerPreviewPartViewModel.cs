using System;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels
{
    public class ContentPickerPreviewPartViewModel
    {
        public string[] ContentItemIds { get; set; } = Array.Empty<string>();

        // [BindNever]
        // public ContentItem ContentItem { get; set; }
        //
        // [BindNever]
        // public ContentPickerPreviewPart ContentPickerPreviewPart { get; set; }
        //
        // [BindNever]
        // public ContentPickerPreviewPartSettings Settings { get; set; }
    }
}
