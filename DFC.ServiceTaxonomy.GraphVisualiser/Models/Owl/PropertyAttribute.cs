using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class PropertyAttribute
    {
        public string Id { get; set; } = string.Empty;
        public string Iri { get; set; } = string.Empty;
        public string BaseIri { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public List<string> Attributes { get; set; } = new List<string>();
        public string Domain { get; set; } = string.Empty;
        public string Range { get; set; } = string.Empty;
    }
}
