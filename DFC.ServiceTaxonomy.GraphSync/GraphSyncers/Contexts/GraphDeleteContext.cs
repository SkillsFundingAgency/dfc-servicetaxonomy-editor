using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphDeleteContext : GraphSyncContext, IGraphDeleteItemSyncContext
    {
        public new IGraphDeleteContext? ParentContext { get; }
        public new IEnumerable<IGraphDeleteContext> ChildContexts => _childContexts.Cast<IGraphDeleteContext>();

        public IDeleteGraphSyncer DeleteGraphSyncer { get; }
        public IDeleteNodeCommand DeleteNodeCommand { get; }
        public DeleteOperation DeleteOperation { get; }
        public IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }

        public GraphDeleteContext(
            ContentItem contentItem,
            IDeleteNodeCommand deleteNodeCommand,
            IDeleteGraphSyncer deleteGraphSyncer,
            DeleteOperation deleteOperation,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties,
            IGraphDeleteContext? parentGraphDeleteContext)
            : base(contentItem, graphSyncHelper, contentManager, contentItemVersion, parentGraphDeleteContext)
        {
            DeleteGraphSyncer = deleteGraphSyncer;
            DeleteNodeCommand = deleteNodeCommand;
            DeleteOperation = deleteOperation;
            DeleteIncomingRelationshipsProperties = deleteIncomingRelationshipsProperties;
        }
    }
}
