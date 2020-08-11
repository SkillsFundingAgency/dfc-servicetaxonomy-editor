using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphDeleteItemSyncContext : IGraphDeleteContext
    {
        new ContentTypePartDefinition ContentTypePartDefinition { get; set; }
    }
}
