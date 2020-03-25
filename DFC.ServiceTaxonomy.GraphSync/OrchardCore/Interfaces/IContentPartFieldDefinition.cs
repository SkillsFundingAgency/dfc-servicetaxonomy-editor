using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces
{
    public interface IContentPartFieldDefinition : IContentDefinition
    {
        ContentFieldDefinition FieldDefinition { get; }
        ContentPartDefinition PartDefinition { get; set; }
    }
}
