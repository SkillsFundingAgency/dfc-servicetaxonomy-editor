using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class Graph : IGraph
    {
        private readonly ILogger<Graph> _logger;
        public string GraphName { get; }
        public bool DefaultGraph { get; }
        public int Instance { get; }
        public IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; internal set; }
        public INeoEndpoint Endpoint { get; }

        private ulong _inFlightCount;

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph, int instance, ILogger<Graph> logger)
        {
            _logger = logger;
            Endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;
            _inFlightCount = 0;

            // GraphReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            GraphReplicaSetLowLevel = default!;
        }

        public ulong InFlightCount => Interlocked.Read(ref _inFlightCount);

        public Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            try
            {
                //todo: check here also if disabled and throw?

                ulong newInFlightCount = Interlocked.Increment(ref _inFlightCount);

                _logger.LogTrace("Running batch of {NumberOfQueries} queries on {GraphName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                    queries.Length, GraphName, Instance, newInFlightCount);

                return Endpoint.Run(queries, GraphName, DefaultGraph);
            }
            finally
            {
                Interlocked.Decrement(ref _inFlightCount);
            }
        }

        public Task Run(params ICommand[] commands)
        {
            try
            {
                //todo: check here also if disabled and throw?

                ulong newInFlightCount = Interlocked.Increment(ref _inFlightCount);

                _logger.LogTrace("Running batch of {NumberOfQueries} commands on {GraphName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                    commands.Length, GraphName, Instance, newInFlightCount);

                return Endpoint.Run(commands, GraphName, DefaultGraph);
            }
            finally
            {
                Interlocked.Decrement(ref _inFlightCount);
            }
        }

        public IGraphReplicaSetLowLevel GetReplicaSetLimitedToThisGraph()
        {
            return GraphReplicaSetLowLevel.CloneLimitedToGraphInstance(Instance);
        }
    }
}
