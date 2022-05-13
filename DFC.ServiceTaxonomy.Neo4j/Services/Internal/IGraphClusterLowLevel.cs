using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal interface IGraphClusterLowLevel : IGraphCluster
    {
        //ImmutableDictionary<string, IGraphReplicaSetLowLevel> GraphReplicaSets { get; }

        IGraphReplicaSetLowLevel GetGraphReplicaSetLowLevel(string replicaSetName);
    }
}
