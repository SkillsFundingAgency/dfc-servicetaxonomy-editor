using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphMergeContext : IGraphOperationContext
    {
        public IGraphReplicaSet GraphReplicaSet { get; }
        IMergeNodeCommand MergeNodeCommand { get; }
        IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }
    }
}
