using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphSyncContext : GraphOperationContext, IGraphSyncContext
    {
        public IGraphSyncContext? ParentContext { get; }

        public IEnumerable<IGraphSyncContext> ChildContexts => _childContexts;
        protected readonly List<IGraphSyncContext> _childContexts;

        protected GraphSyncContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IGraphSyncContext? parentContext)
            : base(contentItem, graphSyncHelper, contentManager, contentItemVersion)
        {
            ParentContext = parentContext;

            _childContexts = new List<IGraphSyncContext>();
        }

        public void AddChildContext(IGraphSyncContext graphSyncContext)
        {
            _childContexts.Add(graphSyncContext);
        }
    }
}
