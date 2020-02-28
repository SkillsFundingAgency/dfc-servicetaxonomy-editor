using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface IGraphDatabase
    {
        Task<List<T>> RunReadQuery<T>(Query query, Func<IRecord, T> operation);

        //todo:read command
        //Task<List<T>> RunReadCommand<T>(ICommand command);

        /// <summary>
        /// Run queries, in order, within a write transaction. No results returned.
        /// </summary>
        Task RunWriteCommands(params ICommand[] commands);
    }
}
