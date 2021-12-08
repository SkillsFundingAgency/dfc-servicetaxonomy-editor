namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    internal interface IGraphClusterLowLevel : IGraphCluster
    {
        IGraphReplicaSetLowLevel GetGraphReplicaSetLowLevel(string replicaSetName);
    }
}
