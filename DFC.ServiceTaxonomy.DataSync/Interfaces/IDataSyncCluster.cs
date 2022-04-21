using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IDataSyncCluster
    {
        IEnumerable<string> DataSyncReplicaSetNames { get; }

        IDataSyncReplicaSet GetDataSyncReplicaSet(string replicaSetName);

        Task<List<T>> Run<T>(string replicaSetName, params IQuery<T>[] queries);

        Task Run(string replicaSetName, params ICommand[] commands);

        Task RunOnAllReplicaSets(params ICommand[] commands);
    }
}
