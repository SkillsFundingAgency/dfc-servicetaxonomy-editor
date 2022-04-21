namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    internal interface IDataSyncClusterLowLevel : IDataSyncCluster
    {
        IDataSyncReplicaSetLowLevel GetDataSyncReplicaSetLowLevel(string replicaSetName);
    }
}
