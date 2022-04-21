using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IValidateAndRepairItemSyncContext : IItemSyncContext, IValidateAndRepairContext
    {
        ContentTypeDefinition ContentTypeDefinition { get; }
        object NodeId { get; }
    }
}
