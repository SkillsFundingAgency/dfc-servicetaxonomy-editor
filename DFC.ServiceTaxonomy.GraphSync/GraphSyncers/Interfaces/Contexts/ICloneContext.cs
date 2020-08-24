using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface ICloneContext
    {
        ContentItem ContentItem { get; set; }
    }
}
