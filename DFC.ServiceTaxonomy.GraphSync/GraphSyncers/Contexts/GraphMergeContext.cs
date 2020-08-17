using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphMergeContext : GraphSyncContext, IGraphMergeItemSyncContext
    {
        //todo: can we use c#9 covariant returns in interfaces?
        public new IGraphMergeContext? ParentContext { get; }
        public new IEnumerable<IGraphMergeContext> ChildContexts => _childContexts.Cast<IGraphMergeContext>();

        public IGraphReplicaSet GraphReplicaSet { get; }
        public IMergeNodeCommand MergeNodeCommand { get; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }

        public GraphMergeContext(
            IGraphSyncHelper graphSyncHelper,
            IGraphReplicaSet graphReplicaSet,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersionFactory contentItemVersionFactory,
            IGraphMergeContext? parentGraphMergeContext)
        : base(contentItem, graphSyncHelper, contentManager, contentItemVersionFactory.Get(graphReplicaSet.Name), parentGraphMergeContext)
        {
            GraphReplicaSet = graphReplicaSet;
            MergeNodeCommand = mergeNodeCommand;
            ReplaceRelationshipsCommand = replaceRelationshipsCommand;
        }
    }
}
