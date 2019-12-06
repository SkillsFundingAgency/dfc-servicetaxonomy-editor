using DFC.ServiceTaxonomy.Editor.Module.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class DisplayGraphUriIdFieldViewModel
    {
        public string? Text => Field?.Text;
        public GraphUriIdField? Field { get; set; }
        public ContentPart? Part { get; set; }
        public ContentPartFieldDefinition? PartFieldDefinition { get; set; }
    }
}
