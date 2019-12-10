using DFC.ServiceTaxonomy.Editor.Module.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class DisplayGraphLookupFieldViewModel
    {
        public string? DisplayText => Field?.DisplayText;
        public string? Value => Field?.Value;
        public GraphLookupField? Field { get; set; }
        public ContentPart? Part { get; set; }
        public ContentPartFieldDefinition? PartFieldDefinition { get; set; }
    }
}
