using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Interfaces
{
    public interface IGraphReplicaSet
    {
        string Name { get; }
        int InstanceCount { get; }

        /// <summary>
        /// Run a query against a replica.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> Run<T>(IQuery<T> query);

        /// <summary>
        /// Run a collection of queries against a replica
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queries">The queries to run</param>
        /// <returns></returns>
        Task<List<T>> Run<T>(IEnumerable<IQuery<T>> queries);

        /// <summary>
        /// Run commands, in order, within a write transaction, against all replicas in the set. No results returned.
        /// </summary>
        /// <param name="commands">The command(s) to run.</param>
        /// <returns></returns>
        Task Run(params ICommand[] commands);
    }
}
