using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    internal class CosmosDbGraphClusterLowLevel : CosmosDbGraphCluster, IGraphClusterLowLevel
    {
        internal CosmosDbGraphClusterLowLevel(IEnumerable<IGraphReplicaSetLowLevel> replicaSets)
            : base(replicaSets)
        {
        }

        public IGraphReplicaSetLowLevel GetGraphReplicaSetLowLevel(string replicaSetName)
        {
            return _graphReplicaSets[replicaSetName];
        }
    }
}
