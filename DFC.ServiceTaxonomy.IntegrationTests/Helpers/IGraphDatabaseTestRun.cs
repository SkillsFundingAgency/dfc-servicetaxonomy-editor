using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public interface IGraphDatabaseTestRun : IDisposable /*: IGraphDatabase << currently, but might change it) */
    {
        Task<List<T>> RunReadQuery<T>(Query query, Func<IRecord, T> operation);
        Task RunWriteQueries(params Query[] queries);

        /// <summary>
        /// useful when debugging test arrangements
        /// </summary>
        Task RunWriteQueriesWithCommit(params Query[] queries);
    }}
