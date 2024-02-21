using DFC.ServiceTaxonomy.CompUi.Enums;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class Processing
    {
        public string? ContentType { get; set; }
        public string? Content { get; set; }
        public string? ContentItemId { get; set; }
        public int DocumentId { get; set; }
        public int Latest { get; set; }
        public int Published { get; set; }
        public ProcessingEvents EventType { get; set; }
    }
}
