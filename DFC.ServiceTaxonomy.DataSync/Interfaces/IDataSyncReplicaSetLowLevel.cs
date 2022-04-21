using DFC.ServiceTaxonomy.DataSync.Enums;
using DFC.ServiceTaxonomy.DataSync.Models;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IDataSyncReplicaSetLowLevel : IDataSyncReplicaSet
    {
        Models.DataSync[] DataSyncInstances { get; }

        IDataSyncReplicaSetLowLevel CloneLimitedToGraphInstance(int instance);

        DisabledStatus Disable(int instance);

        EnabledStatus Enable(int instance);

        //todo: make public here, rather than in IDataSyncReplicaSet
        //int EnabledInstanceCount { get; }
        //bool IsEnabled(int instance);

        string ToTraceString();
    }
}
