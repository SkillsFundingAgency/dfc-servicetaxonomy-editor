using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using DFC.ServiceTaxonomy.Neo4j.Log;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal enum EnabledStatus
    {
        Enabled,
        AlreadyEnabled
    }

    internal enum DisabledStatus
    {
        Disabled,
        AlreadyDisabled,
        ReEnabledDuringQuiesce
    }

    internal class GraphReplicaSetLowLevel : GraphReplicaSet, IGraphReplicaSetLowLevel
    {
        internal GraphReplicaSetLowLevel(
            string name,
            IEnumerable<Graph> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
            : base(name, graphInstances, logger, limitToGraphInstance)
        {
            //todo: looks like calling CloneLimitedToGraphInstance overwrites the pointer back to the graphreplicaset
            foreach (var graph in _graphInstances)
            {
                graph.GraphReplicaSetLowLevel = this;
            }

            if (InstanceCount > 64)
                throw new ArgumentException("A max of 64 graph instances in the replica set is supported.");

            _replicaEnabledFlags = (ulong)BigInteger.Pow(2, InstanceCount) - 1;
        }

        public Graph[] GraphInstances
        {
            get => _graphInstances;
        }

        public IGraphReplicaSetLowLevel CloneLimitedToGraphInstance(int instance)
        {
            return new GraphReplicaSetLowLevel(Name, _graphInstances, _logger, instance);
        }

        private enum QuiesceStatus
        {
            Quiesced,
            QuiesceAbandonedAsReplicaEnabled
        }

        public DisabledStatus Disable(int instance)
        {
            ValidateInstance(instance);

            ulong replicaFlag = 1ul << instance;
            ulong replicaMask = ~replicaFlag;

            _logger.LogInformation("Disabling graph #{Instance} {GraphName}.",
                instance, _graphInstances[instance].GraphName);

            ulong oldReplicaEnabledFlags = Interlocked.And(ref _replicaEnabledFlags, replicaMask);
            bool alreadyDisabled = (oldReplicaEnabledFlags & replicaFlag) == 0;

            _logger.LogInformation("<{LogId}> Disabled graph #{Instance} {GraphName} - Graph was previously {OldEnabledStatus}.",
                LogId.GraphDisabled, instance, _graphInstances[instance].GraphName, alreadyDisabled?"disabled":"enabled");

            if (alreadyDisabled)
            {
                return DisabledStatus.AlreadyDisabled;
            }

            var quiesceStatus = Quiesce(instance);

            if (quiesceStatus == QuiesceStatus.QuiesceAbandonedAsReplicaEnabled)
                return DisabledStatus.ReEnabledDuringQuiesce;

            return DisabledStatus.Disabled;
        }

        private QuiesceStatus Quiesce(int instance)
        {
            //todo: timeout?

            if (IsEnabled(instance))
            {
                _logger.LogInformation("Quiescing graph #{Instance} {GraphName} - Graph enabled, abandoning quiesce.",
                    instance, _graphInstances[instance].GraphName);
                return QuiesceStatus.QuiesceAbandonedAsReplicaEnabled;
            }

            var graphInstance = _graphInstances[instance];

            _logger.LogInformation("<{LogId}> Quiescing graph #{Instance} {GraphName} - {InFlightCount} in-flight queries/commands.",
                LogId.QuiescingGraph, instance, _graphInstances[instance].GraphName, graphInstance.InFlightCount);

            // flush any in-flight commands/queries
            ulong inFlightCount;
            while ((inFlightCount = graphInstance.InFlightCount) > 0)
            {
                _logger.LogInformation("<{LogId}> Quiescing graph #{Instance} {GraphName} - {InFlightCount} in-flight queries/commands.",
                    LogId.QuiescingGraph, instance, _graphInstances[instance].GraphName, inFlightCount);

                if (IsEnabled(instance))
                {
                    _logger.LogInformation("Quiescing graph #{Instance} {GraphName} - Graph re-enabled, abandoning quiesce.",
                        instance, _graphInstances[instance].GraphName);
                    return QuiesceStatus.QuiesceAbandonedAsReplicaEnabled;
                }

                Thread.Sleep(100);
            }

            return QuiesceStatus.Quiesced;
        }

        public EnabledStatus Enable(int instance)
        {
            ValidateInstance(instance);

            ulong replicaFlag = 1ul << instance;

            _logger.LogInformation("Enabling graph #{Instance} {GraphName}.",
                instance, _graphInstances[instance].GraphName);

            ulong oldReplicaEnabledFlags = Interlocked.Or(ref _replicaEnabledFlags, replicaFlag);

            var alreadyEnabled = (oldReplicaEnabledFlags & replicaFlag) != 0
                ? EnabledStatus.AlreadyEnabled
                : EnabledStatus.Enabled;

            _logger.LogInformation("<{LogId}> Enabled graph #{Instance} {GraphName} - Graph was previously {OldEnabledStatus}.",
                LogId.GraphEnabled, instance, _graphInstances[instance].GraphName, alreadyEnabled == EnabledStatus.AlreadyEnabled?"enabled":"disabled");

            return alreadyEnabled;
        }

        private void ValidateInstance(int instance)
        {
            if (instance < 0 || instance >= InstanceCount)
                throw new ArgumentException($"{instance} is not a valid instance, it must be between 0 and {InstanceCount-1}.", nameof(instance));
        }

        public string ToTraceString()
        {
            StringBuilder trace = new();
            trace.AppendLine("replicaSet:");

            ulong replicaEnabledFlags = ReplicaEnabledFlags;

            trace.AppendLine($"  InstanceCount={InstanceCount}, EnabledInstanceCount={EnabledInstanceCount(replicaEnabledFlags)}, ReplicaEnabledFlags={Convert.ToString((long)replicaEnabledFlags, 2)}");
            for (int instance = 0; instance < InstanceCount; ++instance)
            {
                var graph = GraphInstances[instance];

                trace.AppendLine($"    Graph #{instance}: {graph.GraphName}");
                trace.AppendLine($"      IsEnabled()={IsEnabled(replicaEnabledFlags, instance)}");
            }

            return trace.ToString();
        }
    }
}
