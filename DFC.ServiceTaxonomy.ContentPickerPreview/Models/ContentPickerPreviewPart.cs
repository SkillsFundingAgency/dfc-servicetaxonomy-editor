using System;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Models
{
    public class ContentPickerPreviewPart : ContentPart
    {
        public string[] ContentItemIds { get; set; } = Array.Empty<string>();
    }
}
