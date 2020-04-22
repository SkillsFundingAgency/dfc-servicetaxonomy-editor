using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Models.Interface;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface IGraphDatabase
    {
        IEnumerable<INeoDriver> Drivers { get; }
        Task<List<T>> Run<T>(IQuery<T> query, string? endpoint = null);

        /// <summary>
        /// Run queries, in order, within a write transaction. No results returned.
        /// </summary>
        Task Run(params ICommand[] commands);
    }
}
