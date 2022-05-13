using System.Collections.Generic;
#if REPLICA_DISABLING_NET5_ONLY
using System.Threading;
#endif
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class Graph : IGraph
    {
        private readonly ILogger _logger;
        public string GraphName { get; }
        public bool DefaultGraph { get; }
        public int Instance { get; }
        public IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; internal set; }
        public INeoEndpoint Endpoint { get; }

#if REPLICA_DISABLING_NET5_ONLY
        private ulong _inFlightCount;
#endif

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph, int instance, ILogger logger)
        {
            _logger = logger;
            Endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;
#if REPLICA_DISABLING_NET5_ONLY
            _inFlightCount = 0;
#endif

            // GraphReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            GraphReplicaSetLowLevel = default!;
        }

        #if REPLICA_DISABLING_NET5_ONLY
        public ulong InFlightCount => Interlocked.Read(ref _inFlightCount);
        #endif

        public virtual Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
#if REPLICA_DISABLING_NET5_ONLY
            try
            {
                //todo: check here also if disabled and throw?

                ulong newInFlightCount = Interlocked.Increment(ref _inFlightCount);
#else
                ulong newInFlightCount = 0;
#endif

                _logger.LogTrace("Running batch of {NumberOfQueries} queries on {GraphName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                    queries.Length, GraphName, Instance, newInFlightCount);

                return Endpoint.Run(queries, GraphName, DefaultGraph);
#if REPLICA_DISABLING_NET5_ONLY
            }
            finally
            {
                Interlocked.Decrement(ref _inFlightCount);
            }
#endif
        }

        public virtual Task Run(params ICommand[] commands)
        {
#if REPLICA_DISABLING_NET5_ONLY
            try
            {
                //todo: check here also if disabled and throw?

                ulong newInFlightCount = Interlocked.Increment(ref _inFlightCount);
#else
                ulong newInFlightCount = 0;
#endif

                _logger.LogTrace("Running batch of {NumberOfQueries} commands on {GraphName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                    commands.Length, GraphName, Instance, newInFlightCount);

                return Endpoint.Run(commands, GraphName, DefaultGraph);
#if REPLICA_DISABLING_NET5_ONLY
            }
            finally
            {
                Interlocked.Decrement(ref _inFlightCount);
            }
#endif
        }

        public IGraphReplicaSetLowLevel GetReplicaSetLimitedToThisGraph()
        {
            return GraphReplicaSetLowLevel.CloneLimitedToGraphInstance(Instance);
        }
    }
}
