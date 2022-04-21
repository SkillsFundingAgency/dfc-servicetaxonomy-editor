using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Interfaces;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IDataMergeContext : IDataSyncContext
    {
        //todo: can we use c#9 covariant returns in interfaces?
        new IDataMergeContext? ParentContext { get; }
        new IEnumerable<IDataMergeContext> ChildContexts { get; }

        IMergeDataSyncer MergeDataSyncer { get; }

        IDataSyncReplicaSet DataSyncReplicaSet { get; }
        IMergeNodeCommand MergeNodeCommand { get; }
        IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }
        List<ICommand> ExtraCommands { get; }
    }
}
