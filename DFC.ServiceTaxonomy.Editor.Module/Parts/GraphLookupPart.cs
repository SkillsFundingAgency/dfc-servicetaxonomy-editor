using System;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Editor.Module.Parts
{
    public class GraphLookupPart : ContentPart
    {
        //todo: get rid?
        // public string? DisplayText { get; set; }
        // public string? Value { get; set; }
        //todo: rename?
        //public string[] ItemIds { get; set; } = Array.Empty<string>();

        public (string id, string value)[] Nodes { get; set; } = Array.Empty<(string id, string value)>();
    }
}
