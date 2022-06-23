using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    public class Graph : IGraph
    {
        private readonly ILogger<CosmosDbGraphClusterBuilder> _logger;

        public string GraphName { get; }

        public bool DefaultGraph { get; }

        public int Instance { get; }

        public IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; internal set; }

        public IEndpoint Endpoint { get; }

        // ReSharper disable once ContextualLoggerProblem
        public Graph(IEndpoint endpoint, string graphName, bool defaultGraph, int instance, ILogger<CosmosDbGraphClusterBuilder> logger)
        {
            _logger = logger;
            Endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;

            // GraphReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            GraphReplicaSetLowLevel = default!;
        }

        public virtual Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            ulong newInFlightCount = 0;

            _logger.LogTrace("Running batch of {NumberOfQueries} queries on {GraphName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                queries.Length, GraphName, Instance, newInFlightCount);

            return Endpoint.Run(queries, GraphName, DefaultGraph);
        }

        public virtual Task Run(params ICommand[] commands)
        {
            ulong newInFlightCount = 0;

            _logger.LogTrace("Running batch of {NumberOfQueries} commands on {GraphName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                commands.Length, GraphName, Instance, newInFlightCount);

            return Endpoint.Run(commands, GraphName, DefaultGraph);
        }

        public IGraphReplicaSetLowLevel GetReplicaSetLimitedToThisGraph()
        {
            return GraphReplicaSetLowLevel.CloneLimitedToGraphInstance(Instance);
        }
    }
}
