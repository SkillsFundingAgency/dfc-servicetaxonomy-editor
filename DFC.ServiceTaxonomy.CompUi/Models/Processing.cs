using DFC.ServiceTaxonomy.CompUi.Enums;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class Processing
    {
        public string? Author { get; set; }
        public string? ContentType { get; set; }
        public string? ContentItemId { get; set; }
        public int DocumentId { get; set; }
        public int Latest { get; set; }
        public int Published { get; set; }
        public string? CurrentContent { get; set; }
        public string? PreviousContent { get; set; }
        public ProcessingEvents EventType { get; set; }
        public string? FullUrl { get; set; }
        public string? DisplayText { get; set; }
    }
}
