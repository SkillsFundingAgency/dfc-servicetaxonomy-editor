﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public class GraphCluster : IGraphCluster
    {
        //todo: best type of dic?
        private protected readonly ImmutableDictionary<string, IGraphReplicaSetLowLevel> _graphReplicaSets;

        public IEnumerable<string> GraphReplicaSetNames => _graphReplicaSets.Keys;

        internal GraphCluster(IEnumerable<IGraphReplicaSetLowLevel> replicaSets)
        {
            _graphReplicaSets = replicaSets.ToImmutableDictionary(rs => rs.Name);
        }

        public IGraphReplicaSet GetGraphReplicaSet(string replicaSetName)
        {
            //todo: throw nice exception if not found
            return _graphReplicaSets[replicaSetName];
        }

        public Task<List<T>> Run<T>(string replicaSetName, IQuery<T> query)
        {
            return _graphReplicaSets[replicaSetName].Run(query);
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
