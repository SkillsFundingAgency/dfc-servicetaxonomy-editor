using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    //todo: builder for just this for consumers that don't need multiple replicas?
    public class GraphReplicaSet : IGraphReplicaSet
    {
        private protected readonly Graph[] _graphInstances;
        protected readonly ILogger _logger;
        private protected readonly int? _limitToGraphInstance;
        private protected long _replicaEnabledFlags;

        internal GraphReplicaSet(
            string name,
            IEnumerable<Graph> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
        {
            Name = name;
            _graphInstances = graphInstances.ToArray();
            //_enabledInstanceCount =
            InstanceCount = _graphInstances.Length;
            //todo: check in range
            _logger = logger;
            _limitToGraphInstance = limitToGraphInstance;
            _instanceCounter = -1;
        }

        public string Name { get; }
        public int InstanceCount { get; }
        //todo: something wrong with this? check enabled on replicas? store enabled in here, rather than in replicas?
        //public int EnabledInstanceCount => (int)Interlocked.Read(ref _enabledInstanceCount);

        //protected long _enabledInstanceCount;
        private int _instanceCounter;

        internal int EnabledInstanceCount(long replicaEnabledFlags)
        {
            //todo: check conversion
            return (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount(unchecked((ulong)replicaEnabledFlags));
        }

        public Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            Graph? graphInstance;

            long replicaEnabledFlags = ReplicaEnabledFlags;
            int enabledInstanceCount = EnabledInstanceCount(ReplicaEnabledFlags);

            if (_limitToGraphInstance != null)
            {
                if (!IsEnabled(_limitToGraphInstance.Value))
                    throw new InvalidOperationException($"GraphReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} is disabled. ");

                _logger.LogInformation("Running command on locked graph replica instance #{Instance}.", _limitToGraphInstance.Value);

                graphInstance = _graphInstances[_limitToGraphInstance.Value];
            }
            else if (enabledInstanceCount < InstanceCount)
            {
                if (enabledInstanceCount == 0)
                    throw new InvalidOperationException("No enabled replicas to run query against.");

                int enabledInstance = unchecked(++_instanceCounter) % enabledInstanceCount;

                long shiftingReplicaEnabledFlags = replicaEnabledFlags;

                long instance = 0;
                while (enabledInstance > 0)
                {
                    if ((shiftingReplicaEnabledFlags & 1) != 0)
                        --enabledInstance;

                    //todo: need to swap to ulong, otherwise this will do an arithmetic shift rather than a logical shift
                    shiftingReplicaEnabledFlags >>= 1;
                    ++instance;
                }

                _logger.LogInformation("{DisabledReplicaCount} graph replicas in the set are disabled. Running query on enabled replica #{Instance}. Replica set enabled status: {ReplicaSetEnabledStatus}",
                    InstanceCount-enabledInstanceCount, instance, Convert.ToString(replicaEnabledFlags, 2));

                graphInstance = _graphInstances[instance];
            }
            else
            {
                // fast path, simple round-robin read
                graphInstance = _graphInstances[unchecked(++_instanceCounter) % InstanceCount];
            }

            return graphInstance.Run(queries);
        }

        public Task Run(params ICommand[] commands)
        {
            if (_limitToGraphInstance != null)
            {
                if (!IsEnabled(_limitToGraphInstance.Value))
                    throw new InvalidOperationException($"GraphReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} is disabled. ");

                Graph graph = _graphInstances[_limitToGraphInstance.Value];

                return graph.Run(commands);
            }

            // we read flags just the once
            long currentReplicaEnabledFlags = ReplicaEnabledFlags;

            IEnumerable<Graph> commandGraphs = EnabledInstanceCount(currentReplicaEnabledFlags) < InstanceCount
                ? _graphInstances.Where((_, instance) => IsEnabled(currentReplicaEnabledFlags, instance))
                : _graphInstances;

            return Task.WhenAll(commandGraphs.Select(g => g.Run(commands)));
        }

        public override string ToString()
        {
            return Name;
        }

        protected bool IsEnabled(int instance)
        {
            return IsEnabled(ReplicaEnabledFlags, instance);
            // long currentReplicaEnabledFlags = Interlocked.Read(ref _replicaEnabledFlags);
            // return (currentReplicaEnabledFlags & (1 << instance)) != 0;
        }

        private long ReplicaEnabledFlags => Interlocked.Read(ref _replicaEnabledFlags);

        private bool IsEnabled(long replicaEnabledFlags, int instance)
        {
            return (replicaEnabledFlags & (1 << instance)) != 0;
        }
    }
}
