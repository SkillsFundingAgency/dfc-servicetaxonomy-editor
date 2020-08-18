using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IValidateAndRepairItemSyncContext : IItemSyncContext, IValidateAndRepairContext
    {
        ContentTypeDefinition ContentTypeDefinition { get; }
        object NodeId { get; }
    }
}
