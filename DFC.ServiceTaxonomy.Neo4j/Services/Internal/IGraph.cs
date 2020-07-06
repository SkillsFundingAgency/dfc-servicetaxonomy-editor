using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal interface IGraph
    {
        string GraphName { get; }
        bool DefaultGraph { get; }
        int Instance { get; }
        IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; }

        Task<List<T>> Run<T>(IQuery<T> query);
        Task Run(params ICommand[] commands);

        IGraphReplicaSetLowLevel GetReplicaSetLimitedToThisGraph();
    }
}
