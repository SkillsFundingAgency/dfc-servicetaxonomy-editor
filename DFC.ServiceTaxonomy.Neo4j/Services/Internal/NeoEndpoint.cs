using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class NeoEndpoint : INeoEndpoint, IDisposable
    {
        public string Name { get; }
        private readonly IDriver _driver;
        private readonly ILogger<NeoEndpoint> _logger;

        public NeoEndpoint(string endpointName, IDriver driver, ILogger<NeoEndpoint> logger)
        {
            _driver = driver;
            _logger = logger;
            Name = endpointName;
        }

        public async Task<List<T>> Run<T>(IQuery<T> query, string database, bool defaultDatabase)
        {
            LogRun("query", database, defaultDatabase);

            IAsyncSession session = GetAsyncSession(database, defaultDatabase);
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

        public async Task Run(ICommand[] commands, string database, bool defaultDatabase)
        {
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                // transaction functions auto-retry
                //todo: configure retry? timeout? etc.

                LogRun("command(s)", database, defaultDatabase);

                await session.WriteTransactionAsync(async tx =>
                {
                    foreach (ICommand command in commands)
                    {
                        IResultCursor result = await tx.RunAsync(command.Query);

                        var records = await result.ToListAsync(r => r);
                        var resultSummary = await result.ConsumeAsync();

                        _logger.LogDebug(
                            $"Query result available after: {resultSummary.ResultAvailableAfter}, consumed after: {resultSummary.ResultConsumedAfter}");

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

        private IAsyncSession GetAsyncSession(string database, bool defaultDatabase)
        {
            return defaultDatabase ? _driver.AsyncSession()
                : _driver.AsyncSession(builder => builder.WithDatabase(database));
        }

        private void LogRun(string runType, string database, bool defaultDatabase)
        {
            _logger.LogInformation($"Running {runType} on {Name} endpoint, using {(defaultDatabase?"default":database)} database.");
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
