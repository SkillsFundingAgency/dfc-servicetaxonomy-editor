using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class GraphClusterLowLevel : GraphCluster, IGraphClusterLowLevel
    {
        internal GraphClusterLowLevel(IEnumerable<IGraphReplicaSetLowLevel> replicaSets)
            : base(replicaSets)
        {
        }

        // public ImmutableDictionary<string, IGraphReplicaSetLowLevel> GraphReplicaSets
        // {
        //     get => _graphReplicaSets;
        // }

        public IGraphReplicaSetLowLevel GetGraphReplicaSetLowLevel(string replicaSetName)
        {
            return _graphReplicaSets[replicaSetName];
        }

        // public DisabledStatus Disable(string replicaSetName, int instance)
        // {
        //     return GetGraphReplicaSetLowLevel(replicaSetName).Disable(instance);
        // }
        //
        // public EnabledStatus Enable(string replicaSetName, int instance)
        // {
        //     return GetGraphReplicaSetLowLevel(replicaSetName).Enable(instance);
        // }
    }
}
