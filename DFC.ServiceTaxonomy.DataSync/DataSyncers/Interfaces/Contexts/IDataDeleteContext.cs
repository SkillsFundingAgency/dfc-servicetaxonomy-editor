using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IDataDeleteContext : IDataSyncContext
    {
        new IDataDeleteContext? ParentContext { get; }
        new IEnumerable<IDataDeleteContext> ChildContexts { get; }

        IDeleteDataSyncer DeleteDataSyncer { get; }
        IDeleteNodeCommand DeleteNodeCommand { get; }
        SyncOperation SyncOperation { get; }
        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }
    }
}
