using System;
using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class GraphReplicaSetLowLevel : GraphReplicaSet, IGraphReplicaSetLowLevel
    {
        internal GraphReplicaSetLowLevel(string name, IEnumerable<Graph> graphInstances, int? limitToGraphInstance = null)
            : base(name, graphInstances, limitToGraphInstance)
        {
            //todo: looks like calling CloneLimitedToGraphInstance overwrites the pointer back to the graphreplicaset
            foreach (var graph in _graphInstances)
            {
                graph.GraphReplicaSetLowLevel = this;
            }
        }

        public IGraph[] GraphInstances
        {
            get => _graphInstances;
        }

        public IGraphReplicaSetLowLevel CloneLimitedToGraphInstance(int instance)
        {
            return new GraphReplicaSetLowLevel(Name, _graphInstances, instance);
        }

        public void Disable(int instance)
        {
            ValidateInstance(instance);

            _graphInstances[instance].Enabled = false;
        }

        public void Enable(int instance)
        {
            ValidateInstance(instance);

            _graphInstances[instance].Enabled = true;
        }

        private void ValidateInstance(int instance)
        {
            if (instance < 0 || instance >= InstanceCount)
                throw new ArgumentException($"{instance} is not a valid instance, it must be between 0 and {InstanceCount-1}.", nameof(instance));
        }
    }
}
