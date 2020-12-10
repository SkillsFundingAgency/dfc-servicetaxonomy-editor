using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
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

        public DisabledStatus Disable(int instance)
        {
            ValidateInstance(instance);

            // this is tricky to get right, but could have a large impact if it goes wrong (live service outage)
            // so perhaps should...
            // * have separate enable/disable and getstatus commands, and have 2 columns in the ui,
            // one with the result from the enabled/disabled, but also a separate status column
            // have an emergency re-enable all replicas button/command/graph control

            // it doesn't matter if _graphDisabled is true for a window where all graph instances in the replica are enabled
            // the replica code that checks the bool will handle that situation

            // it doesn't matter if _enabledInstanceCount is less that the actual number of enabled graph instances for a window
            // the replica code that checks if _enabledInstanceCount < InstanceCount will handle that situation

            ulong replicaFlag = 1ul << instance;
            ulong replicaMask = ~replicaFlag;

            ulong oldReplicaEnabledFlags = Interlocked.And(ref _replicaEnabledFlags, replicaMask);

            bool alreadyDisabled = (oldReplicaEnabledFlags & replicaFlag) == 0;
            if (alreadyDisabled)
            {
                return DisabledStatus.AlreadyDisabled;
            }

            var quiesceStatus = Quiesce(instance);

            if (quiesceStatus == QuiesceStatus.QuiesceAbandonedAsReplicaEnabled)
                return DisabledStatus.ReEnabledDuringQuiesce;

            return DisabledStatus.Disabled;
        }

        enum QuiesceStatus
        {
            Quiesced,
            QuiesceAbandonedAsReplicaEnabled
        }

        private QuiesceStatus Quiesce(int instance)
        {
            //todo: timeout?

            if (IsEnabled(instance))
            {
                _logger.LogInformation("Quiescing graph #{Instance} - Graph enabled, abandoning quiesce.", instance);
                return QuiesceStatus.QuiesceAbandonedAsReplicaEnabled;
            }

            var graphInstance = _graphInstances[instance];

            // flush any in-flight commands/queries
            ulong inFlightCount;
            while ((inFlightCount = graphInstance.InFlightCount) > 0)
            {
                _logger.LogInformation("Quiescing graph #{Instance} - {InFlightCount} in-flight queries/commands.",
                    instance, inFlightCount);

                if (IsEnabled(instance))
                {
                    _logger.LogInformation("Quiescing graph #{Instance} - Graph re-enabled, abandoning quiesce.", instance);
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

            ulong oldReplicaEnabledFlags = Interlocked.Or(ref _replicaEnabledFlags, replicaFlag);

            return (oldReplicaEnabledFlags & replicaFlag) != 0 ? EnabledStatus.AlreadyEnabled : EnabledStatus.Enabled;

            // you could probably confuse enabling/disabling, but we want to avoid cluster wide locks waiting for
            // any in-flight commands/queries to finish


            // it doesn't matter if _graphDisabled is true for a window where all graph instances in the replica are enabled
            // the replica code that checks the bool will handle that situation
        }

        // public void EmergencyEnableAll()
        // {
        //     //todo: emergency enable all reset,
        //     // reset _enabledInstanceCount (safely!), enable all replicas
        // }

        private void ValidateInstance(int instance)
        {
            if (instance < 0 || instance >= InstanceCount)
                throw new ArgumentException($"{instance} is not a valid instance, it must be between 0 and {InstanceCount-1}.", nameof(instance));
        }
    }
}
