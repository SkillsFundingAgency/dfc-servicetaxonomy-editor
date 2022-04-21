using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.Interfaces;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    public class CosmosDbDataSyncCluster : IDataSyncCluster
    {
        private protected readonly ImmutableDictionary<string, IDataSyncReplicaSetLowLevel> _dataSyncReplicaSets;

        public IEnumerable<string> DataSyncReplicaSetNames => _dataSyncReplicaSets.Keys;

        public CosmosDbDataSyncCluster(IEnumerable<IDataSyncReplicaSetLowLevel> replicaSets)
        {
            _dataSyncReplicaSets = replicaSets.ToImmutableDictionary(rs => rs.Name);
        }

        public IDataSyncReplicaSet GetDataSyncReplicaSet(string replicaSetName)
        {
            //todo: throw nice exception if not found
            return _dataSyncReplicaSets[replicaSetName];
        }

        public Task<List<T>> Run<T>(string replicaSetName, params IQuery<T>[] queries)
        {
            return _dataSyncReplicaSets[replicaSetName].Run(queries);
        }

        public Task Run(string replicaSetName, params ICommand[] commands)
        {
            return _dataSyncReplicaSets[replicaSetName].Run(commands);
        }

        public Task RunOnAllReplicaSets(params ICommand[] commands)
        {
            return Task.WhenAll(_dataSyncReplicaSets.Values.Select(rs => rs.Run(commands)));
        }
    }
}
