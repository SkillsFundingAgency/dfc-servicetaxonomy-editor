namespace DFC.ServiceTaxonomy.VersionCompare.ViewModels
{
    public class PropertyDiffViewModel
    {
        public LanguageType LanguageType { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyLabel { get; set; }
        public string? BaseValue { get; set; }
        public string? CompareValue { get; set; }
        public bool IsHtmlShared { get; set; }
        public string? BaseContentId { get; set; }
        public string? CompareContentId { get; set; }
    }

    public enum LanguageType
    {
        Text,
        Html
    }
}
