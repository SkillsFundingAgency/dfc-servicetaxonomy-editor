using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class GraphReplicaSetLowLevel : GraphReplicaSet, IGraphReplicaSetLowLevel
    {
        internal GraphReplicaSetLowLevel(string name, IEnumerable<IGraph> graphInstances)
            : base(name, graphInstances)
        {
        }

        public IGraph[] GraphInstances
        {
            get => _graphInstances;
        }
    }
}
