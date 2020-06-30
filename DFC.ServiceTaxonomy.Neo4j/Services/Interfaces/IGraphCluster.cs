namespace DFC.ServiceTaxonomy.Neo4j.Services.Interfaces
{
    public interface IGraphCluster
    {
        IGraphReplicaSet GetGraphReplicaSet(string replicaSetName);
        //?
        // Task<List<T>> Run<T>(string graphName, IQuery<T> query, int? instance = null);
        // Task Run(string graphName, params ICommand[] commands);
    }
}
