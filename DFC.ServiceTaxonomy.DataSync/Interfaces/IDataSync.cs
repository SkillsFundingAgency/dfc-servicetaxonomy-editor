using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    internal interface IDataSync
    {
        string DataSyncName { get; }

        bool DefaultDataSync { get; }

        int Instance { get; }

        IDataSyncReplicaSetLowLevel DataSyncReplicaSetLowLevel { get; }

        IEndpoint Endpoint { get; }

        Task<List<T>> Run<T>(params IQuery<T>[] queries);

        Task Run(params ICommand[] commands);

        IDataSyncReplicaSetLowLevel GetReplicaSetLimitedToThisDataSync();
    }
}
