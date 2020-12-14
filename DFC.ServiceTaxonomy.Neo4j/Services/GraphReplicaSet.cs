using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Numerics;
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
        private ulong _instanceCounter;

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
            _instanceCounter = 0;
            _replicaEnabledFlags = (ulong)BigInteger.Pow(2, InstanceCount) - 1;
        }

        public string Name { get; }
        public int InstanceCount { get; }

        public int EnabledInstanceCount()
        {
            return (int)EnabledInstanceCount(ReplicaEnabledFlags);
        }

        internal ulong EnabledInstanceCount(ulong replicaEnabledFlags)
        {
            return System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount(replicaEnabledFlags);
        }

        public virtual Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            Graph? graphInstance;

            ulong replicaEnabledFlags = ReplicaEnabledFlags;
            ulong enabledInstanceCount = EnabledInstanceCount(ReplicaEnabledFlags);

            if (_limitToGraphInstance != null)
            {
                if (!IsEnabled(_limitToGraphInstance.Value))
                    throw new InvalidOperationException($"GraphReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} is disabled. ");

                _logger.LogInformation("Running command on locked graph replica instance #{Instance} {GraphName}.",
                    _limitToGraphInstance.Value, _graphInstances[_limitToGraphInstance.Value].GraphName);

                graphInstance = _graphInstances[_limitToGraphInstance.Value];
            }
            else if ((int)enabledInstanceCount < InstanceCount)
            {
                if (enabledInstanceCount == 0)
                    throw new InvalidOperationException("No enabled replicas to run query against.");

                ulong enabledInstance = Interlocked.Increment(ref _instanceCounter) % enabledInstanceCount;

                ulong shiftingReplicaEnabledFlags = replicaEnabledFlags;

                // Stopwatch stopwatch = Stopwatch.StartNew();
                // //todo: from config
                // var timeout = TimeSpan.Parse("00:02");

                // assumes at least one enabled instance (handled by enabledInstanceCount == 0 check above)
                int instance = 0;
                //while (stopwatch.Elapsed < timeout)
                while (true)
                {
                    if ((shiftingReplicaEnabledFlags & 1ul) != 0 && enabledInstance-- == 0)
                        break;

                    shiftingReplicaEnabledFlags >>= 1;
                    ++instance;
                }

                _logger.LogInformation("{DisabledReplicaCount} graph replicas in the set are disabled. Running query on enabled replica #{Instance} {GraphName}. Replica set enabled status: {ReplicaSetEnabledStatus}",
                    InstanceCount-(int)enabledInstanceCount, instance, _graphInstances[instance].GraphName,
                    Convert.ToString((long)replicaEnabledFlags, 2));

                graphInstance = _graphInstances[instance];
            }
            else
            {
                // fast path, simple round-robin read
                graphInstance = _graphInstances[Interlocked.Increment(ref _instanceCounter) % (ulong)InstanceCount];
            }

            return graphInstance.Run(queries);
        }

        public virtual Task Run(params ICommand[] commands)
        {
            if (_limitToGraphInstance != null)
            {
                if (!IsEnabled(_limitToGraphInstance.Value))
                    throw new InvalidOperationException($"GraphReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} {_graphInstances[_limitToGraphInstance.Value].GraphName} is disabled. ");

                Graph graph = _graphInstances[_limitToGraphInstance.Value];

                return graph.Run(commands);
            }

            // we read flags just the once
            ulong currentReplicaEnabledFlags = ReplicaEnabledFlags;

            // disallow any commands (which can mutate the graphs) if we don't have a full replica set
            // at least until we can recreate any transaction on reenabled replicas (transaction log?)
            if ((int)EnabledInstanceCount(currentReplicaEnabledFlags) < InstanceCount)
                throw new InvalidOperationException($"Running commands when a replica is disabled is not allowed.");

            IEnumerable<Graph> commandGraphs = _graphInstances;

            // IEnumerable<Graph> commandGraphs = EnabledInstanceCount(currentReplicaEnabledFlags) < InstanceCount
            //     ? _graphInstances.Where((_, instance) => IsEnabled(currentReplicaEnabledFlags, instance))
            //     : _graphInstances;

            return Task.WhenAll(commandGraphs.Select(g => g.Run(commands)));
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsEnabled(int instance)
        {
            bool isEnabled = IsEnabled(ReplicaEnabledFlags, instance);

            _logger.LogTrace("Graph replica #{Instance} {GraphName} enabled check: {Enabled}",
                instance, _graphInstances[instance].GraphName, isEnabled);

            return isEnabled;
        }

        private protected ulong ReplicaEnabledFlags => Interlocked.Read(ref _replicaEnabledFlags);

        private protected bool IsEnabled(ulong replicaEnabledFlags, int instance)
        {
            return (replicaEnabledFlags & (1ul << instance)) != 0;
        }
    }
}
