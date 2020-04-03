using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class PropertyAttribute
    {
        public string? Id { get; set; }
        public string? Iri { get; set; }
        public string? BaseIri { get; set; }
        public string? Label { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();
        public string? Domain { get; set; }
        public string? Range { get; set; }
    }
}
