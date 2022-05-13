using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class GraphClusterLowLevel : GraphCluster, IGraphClusterLowLevel
    {
        internal GraphClusterLowLevel(
            IEnumerable<IGraphReplicaSetLowLevel> replicaSets,
            ILogger logger)
            : base(replicaSets, logger)
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
