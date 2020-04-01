namespace DFC.ServiceTaxonomy.ContentPickerPreview.Models
{
    public class ContentPickerPreviewPartSettings
    {
        public string? Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }
        public bool EditButton { get; set; }
        public string[] DisplayedContentTypes { get; set; } = new string[0];
    }
}
