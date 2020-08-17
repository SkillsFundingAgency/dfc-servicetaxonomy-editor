using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphDeleteContext : IGraphSyncContext
    {
        new IGraphDeleteContext? ParentContext { get; }
        new IEnumerable<IGraphDeleteContext> ChildContexts { get; }

        IDeleteGraphSyncer DeleteGraphSyncer { get; }
        IDeleteNodeCommand DeleteNodeCommand { get; }
        DeleteOperation DeleteOperation { get; }
        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }
    }
}
