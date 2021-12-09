using DFC.ServiceTaxonomy.Title.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Title.ViewModels
{
    public class UniqueTitlePartViewModel
    {
        public string? Title { get; set; } 
        public UniqueTitlePart? UniqueTitlePart { get; set; }
        public ContentPart? Part { get; set; }
        public ContentTypePartDefinition? PartDefinition { get; set; }
        public string? Hint { get; set; }
    }
}
