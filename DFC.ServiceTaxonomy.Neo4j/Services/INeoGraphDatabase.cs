using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface IGraphDatabase
    {
        Task<List<T>> RunReadQuery<T>(IQuery<T> query);

        /// <summary>
        /// Run queries, in order, within a write transaction. No results returned.
        /// </summary>
        Task RunWriteCommands(params ICommand[] commands);
    }
}
