using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphDeleteItemSyncContext : GraphOperationContext, IGraphDeleteItemSyncContext
    {
        public GraphDeleteItemSyncContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager)
            : base(contentItem, graphSyncHelper, contentManager)
        {
        }
    }
}
