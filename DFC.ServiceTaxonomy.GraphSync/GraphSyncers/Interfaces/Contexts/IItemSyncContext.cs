using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IItemSyncContext : IGraphOperationContext
    {
        new ContentTypePartDefinition ContentTypePartDefinition { get; set; }
    }
}
