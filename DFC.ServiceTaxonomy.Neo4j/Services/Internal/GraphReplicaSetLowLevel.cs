using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class GraphReplicaSetLowLevel : GraphReplicaSet, IGraphReplicaSetLowLevel
    {
        internal GraphReplicaSetLowLevel(string name, IEnumerable<Graph> graphInstances, int? limitToGraphInstance = null)
            : base(name, graphInstances, limitToGraphInstance)
        {
            foreach (var graph in graphInstances)
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
    }
}
