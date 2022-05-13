using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class TestNeoGraphDatabase : IDisposable
    {
        private readonly IDriver _driver;

        public TestNeoGraphDatabase(IOptionsMonitor<Neo4jOptions> neo4jOptionsMonitor)
        {
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            var neo4jOptions = neo4jOptionsMonitor.CurrentValue;

            //todo: pass logger, see https://github.com/neo4j/neo4j-dotnet-driver
            // o => o.WithLogger(logger)
            var endpointToTest = neo4jOptions.Endpoints.FirstOrDefault(x => x.Enabled);

            if(endpointToTest == null)
            {
                throw new InvalidOperationException("No enabled endpoint in Neo4j configuration.");
            }

            _driver = GraphDatabase.Driver(endpointToTest.Uri, AuthTokens.Basic(endpointToTest.Username, endpointToTest.Password));
        }

        public async Task<IGraphDatabaseTestRun> StartTestRun()
        {
            GraphDatabaseTestRun testRun = new GraphDatabaseTestRun();
            await testRun.Initialise(_driver);
            return testRun;
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        private class GraphDatabaseTestRun : IGraphDatabaseTestRun
        {
            private IAsyncSession? _session;
            private IAsyncTransaction? _transaction;

            public async Task Initialise(IDriver driver)
            {
                _session = driver.AsyncSession();
                _transaction = await _session.BeginTransactionAsync();
            }

            public async Task<List<T>> RunReadQuery<T>(Query query, Func<IRecord, T> operation)
            {
                if (_transaction == null)
                    throw new InvalidOperationException($"{nameof(GraphDatabaseTestRun)} not initialised.");

                IResultCursor result = await _transaction.RunAsync(query);
                return await result.ToListAsync(operation);
            }

            #pragma warning disable S4144
            //todo: read/write? can't seems to specify with BeginTransactionAsync? could have 1 read & 1 write transaction? specify write at the session level? remove distinction and just have RunQuery?
            public async Task<List<T>> RunWriteQuery<T>(Query query, Func<IRecord, T> operation)
            {
                if (_transaction == null)
                    throw new InvalidOperationException($"{nameof(GraphDatabaseTestRun)} not initialised.");

                IResultCursor result = await _transaction.RunAsync(query);
                return await result.ToListAsync(operation);
            }
            #pragma warning restore S4144

            public async Task RunWriteQueries(params Query[] queries)
            {
                if (_transaction == null)
                    throw new InvalidOperationException($"{nameof(GraphDatabaseTestRun)} not initialised.");

                foreach (Query query in queries)
                {
                    await _transaction.RunAsync(query);
                }
            }

            public async Task RunWriteQueriesWithCommit(params Query[] queries)
            {
                if (_transaction == null)
                    throw new InvalidOperationException($"{nameof(GraphDatabaseTestRun)} not initialised.");

                foreach (Query query in queries)
                {
                    await _transaction.RunAsync(query);
                }

                await _transaction!.CommitAsync();
                _transaction = await _session!.BeginTransactionAsync();
            }

            public void Dispose()
            {
                // if you want to see the results of a test in neo's browser...
                //(_transaction!.CommitAsync()).GetAwaiter().GetResult();
                _session?.CloseAsync();
            }
        }
    }
}
