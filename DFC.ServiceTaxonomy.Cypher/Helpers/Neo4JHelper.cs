using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Cypher.Configuration;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Cypher.Helpers
{
    public class Neo4JHelper : INeo4JHelper, IDisposable
    {
        private readonly IDriver _neo4JDriver;
        private IResultCursor _resultCursor;

        public Neo4JHelper(Neo4jConfiguration neo4jConfiguration)
        {
            var authToken = AuthTokens.Basic(neo4jConfiguration.Endpoint.Username, neo4jConfiguration.Endpoint.Password);

            _neo4JDriver = GraphDatabase.Driver(neo4jConfiguration.Endpoint.Uri, authToken);
        }

        public async Task<object> ExecuteCypherQueryInNeo4JAsync(string query, IDictionary<string, object> statementParameters)
        {
            IAsyncSession session = _neo4JDriver.AsyncSession();
            try
            {
                return await session.ReadTransactionAsync(async tx =>
                {
                    _resultCursor = await tx.RunAsync(query, statementParameters);
                    return await GetListOfRecordsAsync();
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        private async Task<object> GetListOfRecordsAsync()
        {
            var records = await _resultCursor.ToListAsync();

            if (records == null || !records.Any())
            {
                return null;
            }

            var neoRecords = records.SelectMany(x => x.Values.Values);

            return neoRecords.FirstOrDefault();
        }

        public async Task<IResultSummary> GetResultSummaryAsync()
        {
            return await _resultCursor.ConsumeAsync();
        }

        public void Dispose()
        {
            _neo4JDriver?.Dispose();
        }
    }
}
