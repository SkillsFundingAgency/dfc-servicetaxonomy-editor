using System;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphLookup.Models
{
    public class GraphLookupPart : ContentPart
    {
        public GraphLookupNode[] Nodes { get; set; } = Array.Empty<GraphLookupNode>();
    }
}
