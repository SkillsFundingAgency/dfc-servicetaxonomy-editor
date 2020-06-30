using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    //todo: rename PoorMansGraphCluster?
    public class GraphCluster : IGraphCluster
    {
        //private readonly INeoEndpoint[] _neoEndpoints;
        //todo: best type of dic?
        private readonly Dictionary<string, IGraphReplicaSet> _graphReplicaSets;

        // public GraphCluster(IEnumerable<INeoEndpoint> endpoints, IEnumerable<IGraphReplicaSet> replicaSets)
        public GraphCluster(IEnumerable<IGraphReplicaSet> replicaSets)
        {
            //_neoEndpoints = endpoints.ToArray();
            _graphReplicaSets = replicaSets.ToDictionary(rs => rs.Name);
        }

        public IGraphReplicaSet GetGraphReplicaSet(string replicaSetName)
        {
            //todo: throw nice exception if not found
            return _graphReplicaSets[replicaSetName];
        }
    }
}
