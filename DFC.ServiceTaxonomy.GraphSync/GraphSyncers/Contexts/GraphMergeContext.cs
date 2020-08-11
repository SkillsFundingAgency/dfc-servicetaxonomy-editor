using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphMergeContext : GraphOperationContext, IGraphMergeItemSyncContext
    {
        public IGraphReplicaSet GraphReplicaSet { get; }
        public IMergeNodeCommand MergeNodeCommand { get; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }

        public IGraphMergeContext? ParentGraphMergeContext { get; }

        public GraphMergeContext(
            IGraphSyncHelper graphSyncHelper,
            IGraphReplicaSet graphReplicaSet,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersionFactory contentItemVersionFactory,
            IGraphMergeContext? parentGraphMergeContext)
        : base(contentItem, graphSyncHelper, contentManager)
        {
            GraphReplicaSet = graphReplicaSet;
            MergeNodeCommand = mergeNodeCommand;
            ReplaceRelationshipsCommand = replaceRelationshipsCommand;
            ParentGraphMergeContext = parentGraphMergeContext;

            ContentItemVersion = contentItemVersionFactory.Get(graphReplicaSet.Name);
        }
    }
}
