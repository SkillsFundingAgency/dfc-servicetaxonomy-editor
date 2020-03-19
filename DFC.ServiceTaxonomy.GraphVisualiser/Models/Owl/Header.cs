using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class Header
    {
        public List<string> Languages { get; set; }
        public Description Title { get; set; }
        public string Iri { get; set; }
        public string Version { get; set; }
        public List<string> Author { get; set; }
        public Description Description { get; set; }
    }
}
