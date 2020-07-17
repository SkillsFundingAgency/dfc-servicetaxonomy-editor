using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IGraphMergeItemSyncContext : IGraphMergeContext
    {
        new ContentTypePartDefinition ContentTypePartDefinition { get; set; }
    }
}
