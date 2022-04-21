using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IItemSyncContext : IDataSyncOperationContext
    {
        new ContentTypePartDefinition ContentTypePartDefinition { get; set; }
    }
}
