using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Editor.Module.Fields
{
    public class GraphLookupField : ContentField
    {
//        public string[] ContentItemIds { get; set; } = Array.Empty<string>();

        public string? DisplayText { get; set; }
        public string? Value { get; set; }
    }
}
