﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Interfaces
{
    public interface IGraphCluster
    {
        IEnumerable<string> GraphReplicaSetNames { get; }

        IGraphReplicaSet GetGraphReplicaSet(string replicaSetName);

        Task<List<T>> Run<T>(string replicaSetName, IQuery<T> query, int? instance = null);
        Task Run(string replicaSetName, params ICommand[] commands);

        Task RunOnAllReplicaSets(params ICommand[] commands);
    }
}
