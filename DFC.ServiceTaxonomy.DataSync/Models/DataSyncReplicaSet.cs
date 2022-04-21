using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.DataSync.Models
{
    public class DataSyncReplicaSet : IDataSyncReplicaSet
    {
        protected readonly ILogger _logger;
        private protected readonly DataSync[] _graphInstances;
        private protected readonly int? _limitToGraphInstance;
        private protected ulong _replicaEnabledFlags;
        private int _instanceCounter;

        public DataSyncReplicaSet(
            string name,
            IEnumerable<DataSync> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
        {
            Name = name;
            _graphInstances = graphInstances.ToArray();
            InstanceCount = _graphInstances.Length;
            _logger = logger;
            _limitToGraphInstance = limitToGraphInstance;
            _instanceCounter = 0;
            _replicaEnabledFlags = (ulong)BigInteger.Pow(2, InstanceCount) - 1;
        }

        public string Name { get; }
        public int InstanceCount { get; }

        public int EnabledInstanceCount()
        {
            return InstanceCount;
        }

        internal ulong EnabledInstanceCount(ulong replicaEnabledFlags)
        {
            return System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount(replicaEnabledFlags);
        }

        public virtual Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            DataSync? graphInstance;

            if (_limitToGraphInstance != null)
            {
                if (!IsEnabled(_limitToGraphInstance.Value))
                    throw new InvalidOperationException($"DataSyncReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} is disabled. ");

                _logger.LogInformation("Running command on locked graph replica instance #{Instance} {DataSyncName}.",
                    _limitToGraphInstance.Value, _graphInstances[_limitToGraphInstance.Value].DataSyncName);

                graphInstance = _graphInstances[_limitToGraphInstance.Value];
            }
            else
            {
                // fast path, simple round-robin read
                graphInstance = _graphInstances[(ulong)Interlocked.Increment(ref _instanceCounter) % (ulong)InstanceCount];
            }

            return graphInstance.Run(queries);
        }

        public virtual Task Run(params ICommand[] commands)
        {
            if (_limitToGraphInstance != null)
            {
                if (!IsEnabled(_limitToGraphInstance.Value))
                    throw new InvalidOperationException($"DataSyncReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} {_graphInstances[_limitToGraphInstance.Value].DataSyncName} is disabled. ");

                DataSync graph = _graphInstances[_limitToGraphInstance.Value];
                return graph.Run(commands);
            }

            IEnumerable<DataSync> commandGraphs = _graphInstances;
            return Task.WhenAll(commandGraphs.Select(g => g.Run(commands)));
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsEnabled(int instance)
        {
            return true;
        }
    }
}
