using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IValidateAndRepairItemSyncContext : IValidateAndRepairContext
    {
        new ContentTypePartDefinition ContentTypePartDefinition { get; set; }

        ContentTypeDefinition ContentTypeDefinition { get; }
        object NodeId { get; }
    }
}
