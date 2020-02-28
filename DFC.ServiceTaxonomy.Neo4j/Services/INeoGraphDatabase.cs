using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface IGraphDatabase
    {
        Task<List<T>> RunReadQuery<T>(Query query, Func<IRecord, T> operation);

        /// <summary>
        /// Run queries, in order, within a write transaction. No results returned.
        /// </summary>
        Task<List<(List<IRecord> records, IResultSummary resultSummary)>> RunWriteQueries(params Query[] queries);
    }
}
