using System;
using System.Collections.Generic;
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
        AlreadyDisabled
    }

    internal class GraphReplicaSetLowLevel : GraphReplicaSet, IGraphReplicaSetLowLevel
    {
        private readonly ILogger _logger;

        internal GraphReplicaSetLowLevel(
            string name,
            IEnumerable<Graph> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
            : base(name, graphInstances, limitToGraphInstance)
        {
            _logger = logger;
            //todo: looks like calling CloneLimitedToGraphInstance overwrites the pointer back to the graphreplicaset
            foreach (var graph in _graphInstances)
            {
                graph.GraphReplicaSetLowLevel = this;
            }
        }

        public Graph[] GraphInstances
        {
            get => _graphInstances;
        }

        public IGraphReplicaSetLowLevel CloneLimitedToGraphInstance(int instance)
        {
            return new GraphReplicaSetLowLevel(Name, _graphInstances, _logger, instance);
        }

        //todo: single status enum?
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

            if (!_graphInstances[instance].Disable())
                return DisabledStatus.AlreadyDisabled;

            //todo: just work off graph instances??
            Interlocked.Decrement(ref _enabledInstanceCount);

            return DisabledStatus.Disabled;
        }

        public EnabledStatus Enable(int instance)
        {
            ValidateInstance(instance);

            // you could probably confuse enabling/disabling, but we want to avoid cluster wide locks waiting for
            // any in-flight commands/queries to finish
            if (_graphInstances[instance].Enable())
                return EnabledStatus.AlreadyEnabled;

            Interlocked.Increment(ref _enabledInstanceCount);

            // it doesn't matter if _graphDisabled is true for a window where all graph instances in the replica are enabled
            // the replica code that checks the bool will handle that situation

            // however, doing this seems dangerous, and we don't want to lock all replica's while a single replica is being enabled/disabled
            //_graphDisabled = _graphInstances.Any(g => !g.Enabled);

            return EnabledStatus.Enabled;
        }

        public bool IsEnabled(int instance)
        {
            return _graphInstances[instance].Enabled;
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
