using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class ClassAttribute
    {
        public string Id { get; set; } = string.Empty;
        public string Iri { get; set; } = string.Empty;
        public string BaseIri { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<string> Attributes { get; set; } = new List<string>();
        public string StaxBackgroundColour { get; set; } = string.Empty;
        public List<string> StaxProperties { get; set; } = new List<string>();
    }
}
