using DFC.ServiceTaxonomy.Editor.Module.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class EditGraphLookupFieldViewModel
    {
        public string? DisplayText { get; set; }
        public string? Value { get; set; }
        public GraphLookupField? Field { get; set; }
        public ContentPart? Part { get; set; }
        public ContentPartFieldDefinition? PartFieldDefinition { get; set; }
    }
}
