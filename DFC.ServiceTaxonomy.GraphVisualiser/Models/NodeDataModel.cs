using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models
{
    public class NodeDataModel
    {
        public string Id { get; set; } = string.Empty;
        public long Key { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<string> StaxProperties { get; set; } = new List<string>();
    }
}
