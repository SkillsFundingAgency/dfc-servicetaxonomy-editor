using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.VersionComparison.Models.Parts
{
    public class TermPart
    {
        public string? ContentItemId { get; set; }
        public string? DisplayText { get; set; }
        public List<TermPart>? Terms { get; set; }
    }
}
