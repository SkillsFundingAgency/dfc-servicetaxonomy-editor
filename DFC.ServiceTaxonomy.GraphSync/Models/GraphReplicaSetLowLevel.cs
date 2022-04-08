using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Enums;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    internal class GraphReplicaSetLowLevel : GraphReplicaSet, IGraphReplicaSetLowLevel
    {
        internal GraphReplicaSetLowLevel(
            string name,
            IEnumerable<Graph> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
            : base(name, graphInstances, logger, limitToGraphInstance)
        {
            foreach (var graph in _graphInstances)
            {
                graph.GraphReplicaSetLowLevel = this;
            }

            if (InstanceCount > 64)
                throw new ArgumentException("A max of 64 graph instances in the replica set is supported.");
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
            throw new NotImplementedException("Replica disabling is not available.");
        }

        public EnabledStatus Enable(int instance)
        {
            throw new NotImplementedException("Replica disabling is not available.");
        }

        public string ToTraceString()
        {
            return "Replica disabling is not available.";
        }
    }
}
