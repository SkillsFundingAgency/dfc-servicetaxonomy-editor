using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public interface IGraphReplicaSet
    {
        string Name { get; }
        int InstanceCount { get; }
        Task<List<T>> Run<T>(IQuery<T> query, int? instance = null);
        Task Run(params ICommand[] commands);    // force all?
    }

    internal interface INeoEndpoint
    {
        Task<List<T>> Run<T>(IQuery<T> query);
        Task Run(ICommand[] commands);
    }

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

        public async Task<List<T>> Run<T>(IQuery<T> query)
        {
            _logger.LogInformation($"Running query on {Name} endpoint.");

            IAsyncSession session = _driver.AsyncSession();
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

        public async Task Run(ICommand[] commands)
        {
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                // transaction functions auto-retry
                //todo: configure retry? timeout? etc.

                _logger.LogInformation($"Running command(s) on {Name} endpoint.");

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

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }

    internal interface IGraph
    {
        INeoEndpoint Endpoint { get; }
        string GraphName { get; }
        bool DefaultGraph { get; }

        Task<List<T>> Run<T>(IQuery<T> query);
    }

    internal class Graph : IGraph
    {
        public INeoEndpoint Endpoint { get; }
        public string GraphName { get; }
        public bool DefaultGraph { get; }

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph)
        {
            Endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
        }

        public Task<List<T>> Run<T>(IQuery<T> query)
        {

        }
    }

    //todo: builder for just this for consumers that don't need multiple replicas
    public class GraphReplicaSet : IGraphReplicaSet
    {
        private readonly IGraph[] _graphInstances;

        internal GraphReplicaSet(string name, IEnumerable<IGraph> graphInstances)
        {
            Name = name;
            _graphInstances = graphInstances.ToArray();
            InstanceCount = _graphInstances.Length;
        }

        public string Name { get; }
        public int InstanceCount { get; }

        private int _currentInstance;

        public Task<List<T>> Run<T>(IQuery<T> query, int? instance = null)
        {
            // round robin might select wrong graph after overflow, but everything will still work
            //todo: unit test overflow
            instance ??= unchecked(++_currentInstance) % InstanceCount;

            _graphInstances[instance.Value].Run(query);
        }

        public Task Run(params ICommand[] commands)
        {
            var commandTasks = _graphInstances.Select(g => g.Run(commands));
        }
    }

    public interface IGraphCluster
    {
        IGraphReplicaSet GetGraphReplicaSet(string replicaSetName);
        //?
        // Task<List<T>> Run<T>(string graphName, IQuery<T> query, int? instance = null);
        // Task Run(string graphName, params ICommand[] commands);
    }

    //todo: rename PoorMansGraphCluster?
    public class GraphCluster : IGraphCluster
    {
        //private readonly INeoEndpoint[] _neoEndpoints;
        //todo: best type of dic?
        private readonly Dictionary<string, IGraphReplicaSet> _graphReplicaSets;

        // public GraphCluster(IEnumerable<INeoEndpoint> endpoints, IEnumerable<IGraphReplicaSet> replicaSets)
        public GraphCluster(IEnumerable<IGraphReplicaSet> replicaSets)
        {
            //_neoEndpoints = endpoints.ToArray();
            _graphReplicaSets = replicaSets.ToDictionary(rs => rs.Name);
        }


        public IGraphReplicaSet GetGraphReplicaSet(string replicaSetName)
        {
            //todo: throw nice exception if not found
            return _graphReplicaSets[replicaSetName];
        }
    }

    public interface IGraphClusterBuilder
    {
    }

    public class GraphClusterBuilder : IGraphClusterBuilder
    {
        private readonly IOptionsMonitor<Neo4jConfiguration> _neo4JConfigurationOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly Neo4j.Driver.ILogger _logger;

        public GraphClusterBuilder(
            IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions,
            IServiceProvider serviceProvider,
            Neo4j.Driver.ILogger logger)
        {
            _neo4JConfigurationOptions = neo4jConfigurationOptions;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public GraphCluster Build(Action<Neo4jConfiguration>? configure = null)
        {
            Neo4jConfiguration? currentConfig = _neo4JConfigurationOptions.CurrentValue;
            //todo: null
            //todo: ok to mutate returned CurrentValue?
            configure?.Invoke(currentConfig);

            //todo: neo4 doesn't encrypt by default (3 did), see https://neo4j.com/docs/driver-manual/current/client-applications/
            // TrustStrategy
            //o=>o.WithEncryptionLevel(EncryptionLevel.None));

            //todo: throw nice exceptions on missing config

            var neoEndpoints = currentConfig.Endpoints.Select(epc =>
                ActivatorUtilities.CreateInstance<NeoEndpoint>(_serviceProvider,
                epc.Name!, GraphDatabase.Driver(
                    epc.Uri, AuthTokens.Basic(epc.Username, epc.Password),
                    o => o.WithLogger(_logger))));

            var graphReplicaSets = currentConfig.ReplicaSets.Select(rsc =>
                new GraphReplicaSet(
                    rsc.ReplicaSetName!,
                    rsc.Instances.Select(gic =>
                        new Graph(
                            neoEndpoints.First(ep => ep.Name == gic.GraphName),
                            gic.GraphName!,
                            gic.DefaultGraph))));

            return new GraphCluster(graphReplicaSets);
        }
    }

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
