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
        private protected ulong _replicaEnabledFlags;

        internal GraphReplicaSet(
            string name,
            IEnumerable<Graph> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
        {
            Name = name;
            _graphInstances = graphInstances.ToArray();
            InstanceCount = _graphInstances.Length;
            _logger = logger;
            //todo: check in range
            _limitToGraphInstance = limitToGraphInstance;
            _instanceCounter = -1;
        }

        public string Name { get; }
        public int InstanceCount { get; }

        private int _instanceCounter;

        public int EnabledInstanceCount()
        {
            return EnabledInstanceCount(ReplicaEnabledFlags);
        }

        internal int EnabledInstanceCount(ulong replicaEnabledFlags)
        {
            return (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount(replicaEnabledFlags);
        }

        public Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            Graph? graphInstance;

            ulong replicaEnabledFlags = ReplicaEnabledFlags;
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

                ulong shiftingReplicaEnabledFlags = replicaEnabledFlags;

                // assumes at least one enabled instance (handled by enabledInstanceCount == 0 check above)
                int instance = 0;
                while (true)
                {
                    if ((shiftingReplicaEnabledFlags & 1ul) != 0 && enabledInstance-- == 0)
                        break;

                    shiftingReplicaEnabledFlags >>= 1;
                    ++instance;
                }

                _logger.LogInformation("{DisabledReplicaCount} graph replicas in the set are disabled. Running query on enabled replica #{Instance}. Replica set enabled status: {ReplicaSetEnabledStatus}",
                    InstanceCount-enabledInstanceCount, instance, Convert.ToString((long)replicaEnabledFlags, 2));

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
            ulong currentReplicaEnabledFlags = ReplicaEnabledFlags;

            IEnumerable<Graph> commandGraphs = EnabledInstanceCount(currentReplicaEnabledFlags) < InstanceCount
                ? _graphInstances.Where((_, instance) => IsEnabled(currentReplicaEnabledFlags, instance))
                : _graphInstances;

            return Task.WhenAll(commandGraphs.Select(g => g.Run(commands)));
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsEnabled(int instance)
        {
            bool isEnabled = IsEnabled(ReplicaEnabledFlags, instance);

            _logger.LogTrace("Graph replica #{Instance} enabled check: {Enabled}",
                instance, isEnabled);

            return isEnabled;
        }

        private protected ulong ReplicaEnabledFlags => Interlocked.Read(ref _replicaEnabledFlags);

        private protected bool IsEnabled(ulong replicaEnabledFlags, int instance)
        {
            return (replicaEnabledFlags & (1ul << instance)) != 0;
        }
    }
}
