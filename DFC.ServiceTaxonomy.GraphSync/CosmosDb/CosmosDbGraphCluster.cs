using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbGraphCluster : IGraphCluster
    {
        private protected readonly ImmutableDictionary<string, IGraphReplicaSetLowLevel> _graphReplicaSets;

        public IEnumerable<string> GraphReplicaSetNames => _graphReplicaSets.Keys;

        public CosmosDbGraphCluster(IEnumerable<IGraphReplicaSetLowLevel> replicaSets)
        {
            _graphReplicaSets = replicaSets.ToImmutableDictionary(rs => rs.Name);
        }

        public IGraphReplicaSet GetGraphReplicaSet(string replicaSetName)
        {
            //todo: throw nice exception if not found
            return _graphReplicaSets[replicaSetName];
        }

        public Task<List<T>> Run<T>(string replicaSetName, params IQuery<T>[] queries)
        {
            return _graphReplicaSets[replicaSetName].Run(queries);
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
