using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface INeoGraphDatabase
    {
        Task<List<T>> RunReadStatement<T>(Statement statement, Func<IRecord, T> operation);

        /// <summary>
        /// Run statements, in order, within a write transaction. No results returned.
        /// </summary>
        Task RunWriteStatements(params Statement[] statements);
    }
}
