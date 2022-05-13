using DFC.ServiceTaxonomy.GraphSync.Enums;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IGraphReplicaSetLowLevel : IGraphReplicaSet
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
