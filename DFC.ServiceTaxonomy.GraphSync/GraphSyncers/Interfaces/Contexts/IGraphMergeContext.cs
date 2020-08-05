using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    //todo: merge (partly?) with graphsynchelper??
    public interface IGraphMergeContext : IGraphOperationContext
    {
        //todo: move into IGraphOperationContext?
        ContentItem ContentItem { get; }

        public IGraphReplicaSet GraphReplicaSet { get; }
        IMergeNodeCommand MergeNodeCommand { get; }
        IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }

        IGraphMergeContext? ParentGraphMergeContext { get; }
    }
}
