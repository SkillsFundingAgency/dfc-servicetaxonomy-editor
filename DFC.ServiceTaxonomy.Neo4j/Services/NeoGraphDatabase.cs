using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using ILogger = Neo4j.Driver.ILogger;
using DFC.ServiceTaxonomy.Neo4j.Models.Interface;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    // https://github.com/neo4j/neo4j-dotnet-driver
    // https://neo4j.com/docs/driver-manual/4.0/
    public class NeoGraphDatabase : IGraphDatabase, IDisposable
    {
        private readonly ILogger<NeoGraphDatabase> _logger;
        private readonly IEnumerable<INeoDriver> _drivers;
        private int _currentDriver;

        public NeoGraphDatabase(
            INeoDriverBuilder driverBuilder,
            ILogger neoLogger,
            ILogger<NeoGraphDatabase> logger)
        {
            _logger = logger;
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            _drivers = driverBuilder.Build();
            _currentDriver = 0;
        }

        public IEnumerable<INeoDriver> Drivers => _drivers;

        public async Task<List<T>> Run<T>(IQuery<T> query, string? endpoint = null)
        {
            var neoDriver = string.IsNullOrWhiteSpace(endpoint) ?
                GetNextDriver() :
                GetDriverByEndpoint(endpoint);

            _logger.LogInformation($"Executing Query to: {neoDriver.Uri}");

            IAsyncSession session = neoDriver.Driver.AsyncSession();
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
            var executionTasks = _drivers.Select(x => ExecuteTransaction(commands, x));
            await Task.WhenAll(executionTasks);
        }

        private async Task ExecuteTransaction(ICommand[] commands, INeoDriver neoDriver)
        {
            IAsyncSession session = neoDriver.Driver.AsyncSession();
            try
            {
                // transaction functions auto-retry
                //todo: configure retry? timeout? etc.

                _logger.LogInformation($"Executing commands to: {neoDriver.Uri}");

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

        private INeoDriver GetNextDriver()
        {
            if (_currentDriver == _drivers.Count())
            {
                _currentDriver = 0;
            }

            var driverToUse = _drivers.ElementAt(_currentDriver);
            _currentDriver++;

            return driverToUse;
        }

        private INeoDriver GetDriverByEndpoint(string endpoint)
        {
            return _drivers.Single(x => x.Uri == endpoint);
        }

        public void Dispose()
        {
            foreach (var neoDriver in _drivers)
            {
                neoDriver.Driver?.Dispose();
            }
        }
    }
}
