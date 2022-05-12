using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IGraphCluster
    {
        IEnumerable<string> GraphReplicaSetNames { get; }

        IGraphReplicaSet GetGraphReplicaSet(string replicaSetName);

        Task<List<T>> Run<T>(string replicaSetName, params IQuery<T>[] queries);

        Task Run(string replicaSetName, params ICommand[] commands);

        Task RunOnAllReplicaSets(params ICommand[] commands);
    }
}
