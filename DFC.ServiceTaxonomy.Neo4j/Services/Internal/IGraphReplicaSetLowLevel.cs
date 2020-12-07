using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal interface IGraphReplicaSetLowLevel : IGraphReplicaSet
    {
        IGraph[] GraphInstances { get; }

        IGraphReplicaSetLowLevel CloneLimitedToGraphInstance(int instance);

        int EnabledInstanceCount { get; }

        DisabledStatus Disable(int instance);
        EnabledStatus Enable(int instance);

        bool IsEnabled(int instance);
    }
}
