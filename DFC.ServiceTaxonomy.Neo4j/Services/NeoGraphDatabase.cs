using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using ILogger = Neo4j.Driver.ILogger;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    // https://github.com/neo4j/neo4j-dotnet-driver
    // https://neo4j.com/docs/driver-manual/4.0/
    public class NeoGraphDatabase : IGraphDatabase, IDisposable
    {
        private readonly ILogger<NeoGraphDatabase> _logger;
        private readonly IDriver _primaryDriver;
        private readonly IDriver _secondaryDriver;

        public NeoGraphDatabase(
            IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions,
            ILogger neoLogger,
            ILogger<NeoGraphDatabase> logger)
        {
            _logger = logger;
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            var neo4jConfiguration = neo4jConfigurationOptions.CurrentValue;

            _primaryDriver = GraphDatabase.Driver(
                neo4jConfiguration.Endpoint.Uri,
                AuthTokens.Basic(neo4jConfiguration.Endpoint.Username, neo4jConfiguration.Endpoint.Password),
                o => o.WithLogger(neoLogger));

            _secondaryDriver = GraphDatabase.Driver(
#pragma warning disable S1075 // URIs should not be hardcoded
                "bolt://localhost:7687",
#pragma warning restore S1075 // URIs should not be hardcoded
                AuthTokens.Basic("neo4j", "test"),
                o => o.WithLogger(neoLogger));
        }

        public async Task<List<T>> Run<T>(IQuery<T> query)
        {
            IAsyncSession session = _primaryDriver.AsyncSession();
            try
            {
                return await session.ReadTransactionAsync(async tx =>
                {
                    IResultCursor result = await tx.RunAsync(query.Query);
                    return await result.ToListAsync(query.ProcessRecord);
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task Run(params ICommand[] commands)
        {
            await ExecuteTransaction(commands, _primaryDriver, "Primary Graph");
            await ExecuteTransaction(commands, _secondaryDriver, "Secondary Graph");
        }

        private async Task ExecuteTransaction(ICommand[] commands, IDriver driver, string graphName)
        {
            IAsyncSession session = driver.AsyncSession();
            try
            {
                // transaction functions auto-retry
                //todo: configure retry? timeout? etc.

                _logger.LogInformation($"Executing commands to: {graphName}");

                await session.WriteTransactionAsync(async tx =>
                {
                    foreach (ICommand command in commands)
                    {
                        IResultCursor result = await tx.RunAsync(command.Query);

                        var records = await result.ToListAsync(r => r);
                        var resultSummary = await result.ConsumeAsync();

                        _logger.LogInformation($"Query result available after: {resultSummary.ResultAvailableAfter}, consumed after: {resultSummary.ResultConsumedAfter}");

                        if (resultSummary.Notifications.Any())
                        {
                            _logger.LogWarning(
                                $"Query had notifications{Environment.NewLine}:{string.Join(Environment.NewLine, resultSummary.Notifications)}");
                        }

                        command.ValidateResults(records, resultSummary);
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
            _primaryDriver?.Dispose();
            _secondaryDriver?.Dispose();
        }
    }
}
