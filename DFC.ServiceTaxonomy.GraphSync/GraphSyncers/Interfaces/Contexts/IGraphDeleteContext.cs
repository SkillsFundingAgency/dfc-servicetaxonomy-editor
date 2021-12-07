using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphDeleteContext : IGraphSyncContext
    {
        new IGraphDeleteContext? ParentContext { get; }
        new IEnumerable<IGraphDeleteContext> ChildContexts { get; }

        IDeleteGraphSyncer DeleteGraphSyncer { get; }
        IDeleteNodeCommand DeleteNodeCommand { get; }
        SyncOperation SyncOperation { get; }
        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }
    }
}
