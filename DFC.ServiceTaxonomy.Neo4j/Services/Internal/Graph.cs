using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class Graph : IGraph
    {
        public string GraphName { get; }
        public bool DefaultGraph { get; }
        public int Instance { get; }
        public IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; internal set; }
        public INeoEndpoint Endpoint { get; }

        private long _inFlightCount;
        //todo: do we need thread safety for the bool?
        private bool _enabled;

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                if (!value)
                {
                    //todo: timeout?
                    //todo: best way to synchronise?
                    //todo: exit loop if replica reenabled before all in-flight finished
                    // flush any in-flight commands/queries
                    while (Interlocked.Read(ref _inFlightCount) > 0)
                    {
                        if (_enabled)
                            break;

                        Thread.Sleep(100);
                    }
                }
            }
        }

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph, int instance)
        {
            Endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;
            _enabled = true;
            _inFlightCount = 0;

            // GraphReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            GraphReplicaSetLowLevel = default!;
        }

        public Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            try
            {
                Interlocked.Increment(ref _inFlightCount);
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
                Interlocked.Increment(ref _inFlightCount);
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
