using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphSyncContext : GraphOperationContext, IGraphSyncContext
    {
        //todo: provide subclass in derived?
        public IGraphSyncContext? ParentContext { get; }

        protected GraphSyncContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IGraphSyncContext? parentContext)
            : base(contentItem, graphSyncHelper, contentManager, contentItemVersion)
        {
            ParentContext = parentContext;
        }
    }
}
