namespace DFC.ServiceTaxonomy.VersionComparison.Models.Parts
{
    public class HtmlBodyPart
    {
        public string? Html { get; set; }
    }

    public class SharedContent
    {
        public string[]? ContentItemIds { get; set; }
    }

    public class HtmlShared
    {
        public SharedContent? SharedContent { get; set; }
    }

    public class Widget
    {
        public string? ContentItemId { get; set; }
        public string? ContentType { get; set; }
        public HtmlBodyPart? HtmlBodyPart { get; set; }
        public HtmlShared? HTMLShared { get; set; }
    }
}
