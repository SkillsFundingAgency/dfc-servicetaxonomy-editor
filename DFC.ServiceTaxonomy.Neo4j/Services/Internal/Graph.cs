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
        private long _enabled;

        private const long _enabledValue = 1;
        private const long _disabledValue = 0;

        public bool Enabled => Interlocked.Read(ref _enabled) == _enabledValue;

        public bool Enable()
        {
            return Interlocked.Exchange(ref _enabled, _enabledValue) == _enabledValue;
        }

        public bool Disable()
        {
            bool wasEnabled = Interlocked.Exchange(ref _enabled, _disabledValue) == _enabledValue;

            if (!wasEnabled)
                return wasEnabled;

            //todo: timeout?
            //todo: best way to synchronise?
            //todo: exit loop if replica reenabled before all in-flight finished
            // flush any in-flight commands/queries
            while (Interlocked.Read(ref _inFlightCount) > 0)
            {
                if (Interlocked.Read(ref _enabled) == _enabledValue)
                    break;

                Thread.Sleep(100);
            }

            return wasEnabled;
        }

        // public bool Enabled
        // {
        //     get
        //     {
        //         return _enabled;
        //     }
        //     set
        //     {
        //         _enabled = value;
        //         if (!value)
        //         {
        //             //todo: timeout?
        //             //todo: best way to synchronise?
        //             //todo: exit loop if replica reenabled before all in-flight finished
        //             // flush any in-flight commands/queries
        //             while (Interlocked.Read(ref _inFlightCount) > 0)
        //             {
        //                 if (_enabled)
        //                     break;
        //
        //                 Thread.Sleep(100);
        //             }
        //         }
        //     }
        // }

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph, int instance)
        {
            Endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;
            //_enabled = true;
            _enabled = _enabledValue;
            _inFlightCount = 0;

            // GraphReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            GraphReplicaSetLowLevel = default!;
        }

        public Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            try
            {
                //todo: check here also if disabled and throw?

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
                //todo: check here also if disabled and throw?

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
