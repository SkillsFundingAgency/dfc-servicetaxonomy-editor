using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface IGraphDatabase
    {
        // leave or remove??
        Task<List<T>> RunReadQuery<T>(Query query, Func<IRecord, T> operation);

        Task<List<T>> RunReadQuery<T>(IQuery<T> query);

        /// <summary>
        /// Run queries, in order, within a write transaction. No results returned.
        /// </summary>
        Task RunWriteCommands(params ICommand[] commands);
    }
}
