using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IEndpoint
    {
        public string Name { get; set; }

        Task<List<T>> Run<T>(IQuery<T>[] queries, string graphName, bool defaultGraph);

        Task Run(ICommand[] commands, string databaseName, bool defaultDatabase);
    }
}
