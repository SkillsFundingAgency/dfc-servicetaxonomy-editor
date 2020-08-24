using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class CloneContext : ICloneContext
    {
        public ContentItem ContentItem { get; set; }

        public CloneContext(ContentItem contentItem) => ContentItem = contentItem;
    }
}
