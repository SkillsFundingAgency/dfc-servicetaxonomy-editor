using System;
using DFC.ServiceTaxonomy.Editor.Module.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Editor.Module.Parts
{
    public class GraphLookupPart : ContentPart
    {
        public GraphLookupNode[] Nodes { get; set; } = Array.Empty<GraphLookupNode>();
    }
}
