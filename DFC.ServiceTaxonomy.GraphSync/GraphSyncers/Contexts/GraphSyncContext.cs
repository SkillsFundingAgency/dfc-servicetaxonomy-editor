using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
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
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IGraphSyncContext? parentContext,
            ILogger logger)
            : base(contentItem, syncNameProvider, contentManager, contentItemVersion, logger)
        {
            ParentContext = parentContext;

            _childContexts = new List<IGraphSyncContext>();
        }

        public void AddChildContext(IGraphSyncContext graphSyncContext)
        {
            _logger.LogDebug("Adding child to {Context}: {ChildContext}.", ToString(), graphSyncContext.ToString());

            _childContexts.Add(graphSyncContext);
        }
    }
}
