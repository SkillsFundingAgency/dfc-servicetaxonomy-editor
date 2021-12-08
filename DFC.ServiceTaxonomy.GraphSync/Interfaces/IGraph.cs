using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    internal interface IGraph
    {
        string GraphName { get; }

        bool DefaultGraph { get; }

        int Instance { get; }

        IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; }

        IEndpoint Endpoint { get; }

        Task<List<T>> Run<T>(params IQuery<T>[] queries);

        Task Run(params ICommand[] commands);

        IGraphReplicaSetLowLevel GetReplicaSetLimitedToThisGraph();
    }
}
