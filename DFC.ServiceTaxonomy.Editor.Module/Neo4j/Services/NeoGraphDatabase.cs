using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services
{
    //todo: add title to qcflevel rather than numeric (or have both?)
    //todo: create initial esco occupation content type
    //todo: ensure sync to neo when importing data through recipe

    // https://github.com/neo4j/neo4j-dotnet-driver
    // (not updated for 4 yet: https://neo4j.com/docs/driver-manual/1.7/get-started/)
    public class NeoGraphDatabase : INeoGraphDatabase, IDisposable
    {
        private readonly IDriver _driver;

        public NeoGraphDatabase(IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions)
        {
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            var neo4jConfiguration = neo4jConfigurationOptions.CurrentValue;

            _driver = GraphDatabase.Driver(neo4jConfiguration.Endpoint.Uri, AuthTokens.Basic(neo4jConfiguration.Endpoint.Username, neo4jConfiguration.Endpoint.Password));
        }

        public async Task RunWriteStatements(params Statement[] statements)
        {
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                // transaction functions auto-retry
                //todo: configure retry? timeout? etc.
                await session.WriteTransactionAsync(async tx =>
                {
                    foreach (Statement statement in statements)
                    {
                        await tx.RunAsync(statement);
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
