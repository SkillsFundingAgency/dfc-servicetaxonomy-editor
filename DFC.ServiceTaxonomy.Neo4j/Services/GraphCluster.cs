using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    //todo: rename PoorMansGraphCluster?
    public class GraphCluster : IGraphCluster
    {
        //todo: best type of dic?
        private readonly ImmutableDictionary<string, IGraphReplicaSet> _graphReplicaSets;

        public GraphCluster(IEnumerable<IGraphReplicaSet> replicaSets)
        {
            _graphReplicaSets = replicaSets.ToImmutableDictionary(rs => rs.Name);
        }

        public IGraphReplicaSet GetGraphReplicaSet(string replicaSetName)
        {
            //todo: throw nice exception if not found
            return _graphReplicaSets[replicaSetName];
        }

        public Task<List<T>> Run<T>(string replicaSetName, IQuery<T> query, int? instance = null)
        {
            return _graphReplicaSets[replicaSetName].Run(query, instance);
        }

        public Task Run(string replicaSetName, params ICommand[] commands)
        {
            return _graphReplicaSets[replicaSetName].Run(commands);
        }

        public Task RunOnAllReplicaSets(params ICommand[] commands)
        {
            return Task.WhenAll(_graphReplicaSets.Values.Select(rs => rs.Run(commands)));
        }
    }
}
