using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IDataSyncReplicaSet
    {
        string Name { get; }
        int InstanceCount { get; }

        //todo: these 2 should only be public on IDataSyncReplicaSetLowLevel
        bool IsEnabled(int instance);

        int EnabledInstanceCount();

        /// <summary>
        /// Run a collection of queries against a replica
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queries">The queries to run</param>
        /// <returns></returns>
        Task<List<T>> Run<T>(params IQuery<T>[] queries);

        /// <summary>
        /// Run commands, in order, within a write transaction, against all replicas in the set. No results returned.
        /// </summary>
        /// <param name="commands">The command(s) to run.</param>
        /// <returns></returns>
        Task Run(params ICommand[] commands);
    }
}
