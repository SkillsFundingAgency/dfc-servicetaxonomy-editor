using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    public class DataSyncContext : DataSyncOperationContext, IDataSyncContext
    {
        public IDataSyncContext? ParentContext { get; }

        public IEnumerable<IDataSyncContext> ChildContexts => _childContexts;
        protected readonly List<IDataSyncContext> _childContexts;

        protected DataSyncContext(
            ContentItem contentItem,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IDataSyncContext? parentContext,
            ILogger logger)
            : base(contentItem, syncNameProvider, contentManager, contentItemVersion, logger)
        {
            ParentContext = parentContext;
            parentContext?.AddChildContext(this);

            _childContexts = new List<IDataSyncContext>();
        }

        public void AddChildContext(IDataSyncContext dataSyncContext)
        {
            _logger.LogDebug("Adding child to {Context}: {ChildContext}.", ToString(), dataSyncContext.ToString());

            _childContexts.Add(dataSyncContext);
        }
    }
}
