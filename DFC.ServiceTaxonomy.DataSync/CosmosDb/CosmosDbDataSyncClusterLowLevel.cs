using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Interfaces;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    internal class CosmosDbDataSyncClusterLowLevel : CosmosDbDataSyncCluster, IDataSyncClusterLowLevel
    {
        internal CosmosDbDataSyncClusterLowLevel(IEnumerable<IDataSyncReplicaSetLowLevel> replicaSets)
            : base(replicaSets)
        {
        }

        public IDataSyncReplicaSetLowLevel GetDataSyncReplicaSetLowLevel(string replicaSetName)
        {
            return _dataSyncReplicaSets[replicaSetName];
        }
    }
}
