using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services
{
    public interface INeoGraphDatabase
    {
        /// <summary>
        /// Run statements, in order, within a write transaction. No results returned.
        /// </summary>
        Task RunWriteStatements(params Statement[] statements);
    }
}
