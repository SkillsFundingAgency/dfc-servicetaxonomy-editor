using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal interface IGraphReplicaSetLowLevel : IGraphReplicaSet
    {
        IGraph[] GraphInstances { get; }
    }
}
