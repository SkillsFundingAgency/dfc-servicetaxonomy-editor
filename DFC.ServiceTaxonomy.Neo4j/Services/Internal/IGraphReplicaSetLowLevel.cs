using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal interface IGraphReplicaSetLowLevel : IGraphReplicaSet
    {
        Graph[] GraphInstances { get; }

        IGraphReplicaSetLowLevel CloneLimitedToGraphInstance(int instance);

        DisabledStatus Disable(int instance);
        EnabledStatus Enable(int instance);

        //todo: make public here, rather than in IGraphReplicaSet
        //int EnabledInstanceCount { get; }
        //bool IsEnabled(int instance);

        string ToTraceString();
    }
}
