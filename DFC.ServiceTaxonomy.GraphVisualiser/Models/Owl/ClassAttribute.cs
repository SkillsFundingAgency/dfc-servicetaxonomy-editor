using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class ClassAttribute
    {
        public string Id { get; set; }
        public string Iri { get; set; }
        public string BaseIri { get; set; }
        public string Label { get; set; }
        public string Comment { get; set; }
        public List<string> Attributes { get; set; } = new List<string> { };
        public string StaxBackgroundColour { get; set; }
        public List<string> StaxProperties { get; set; }
    }
}
