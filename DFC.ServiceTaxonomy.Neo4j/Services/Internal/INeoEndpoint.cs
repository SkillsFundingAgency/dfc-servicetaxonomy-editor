using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal interface INeoEndpoint
    {
        string Name { get; }

        Task<List<T>> Run<T>(IQuery<T> query, string databaseName, bool defaultDatabase);
        Task<List<T>> Run<T>(IEnumerable<IQuery<T>> queries, string databaseName, bool defaultDatabase);
        Task Run(ICommand[] commands, string databaseName, bool defaultDatabase);
    }
}
