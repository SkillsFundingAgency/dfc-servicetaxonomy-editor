namespace DFC.ServiceTaxonomy.ContentPickerPreview.Settings
{
    public class ContentPickerPreviewPartSettings
    {
        public string? Hint { get; set; }
        public bool Required { get; set; }
        //default to single for now
        //public bool Multiple { get; set; }
        public bool EditButton { get; set; }
        //todo: just allow 1 ContentType
        public bool DisplayAllContentTypes { get; set; }
        public string[] DisplayedContentTypes { get; set; } = new string[0];
    }
}
