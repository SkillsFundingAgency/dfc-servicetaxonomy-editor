using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    //todo: ensure sync to neo when importing data through recipe

    // https://github.com/neo4j/neo4j-dotnet-driver
    // (not updated for 4 yet: https://neo4j.com/docs/driver-manual/1.7/get-started/)
    public class NeoGraphDatabase : IGraphDatabase, IDisposable
    {
        private readonly IDriver _driver;

        public NeoGraphDatabase(
            IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions,
            ILogger logger)
        {
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            var neo4jConfiguration = neo4jConfigurationOptions.CurrentValue;

            _driver = GraphDatabase.Driver(
                neo4jConfiguration.Endpoint.Uri,
                AuthTokens.Basic(neo4jConfiguration.Endpoint.Username, neo4jConfiguration.Endpoint.Password),
                o => o.WithLogger(logger));
        }

        public async Task<List<T>> RunReadQuery<T>(Query query, Func<IRecord, T> operation)
        {
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                return await session.ReadTransactionAsync(async tx =>
                {
                    IResultCursor result = await tx.RunAsync(query);
                    return await result.ToListAsync(operation);
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task RunWriteQueries(params Query[] queries)
        {
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                // transaction functions auto-retry
                //todo: configure retry? timeout? etc.
                await session.WriteTransactionAsync(async tx =>
                {
                    foreach (Query query in queries)
                    {
                        await tx.RunAsync(query);
                    }
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
