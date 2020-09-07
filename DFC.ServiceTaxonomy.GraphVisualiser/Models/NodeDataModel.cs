using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models
{
    public class NodeDataModel
    {
        public string? Id { get; set; }
        public long Key { get; set; }
        public string? Type { get; set; }
        public string? Label { get; set; }
        public string? Comment { get; set; }
        public string? ContentItemID { get; set; }
        public List<string> StaxProperties { get; set; } = new List<string>();
    }
}
