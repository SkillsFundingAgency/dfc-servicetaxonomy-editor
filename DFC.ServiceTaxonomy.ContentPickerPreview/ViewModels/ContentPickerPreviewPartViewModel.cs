using System;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        [BindNever]
        public ContentPickerPreviewPartSettings? Settings { get; set; }
    }
}
