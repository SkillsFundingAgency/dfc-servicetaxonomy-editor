using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphDeleteContext : IGraphOperationContext
    {
        IDeleteGraphSyncer DeleteGraphSyncer { get; }
        IDeleteNodeCommand DeleteNodeCommand { get; }
        DeleteOperation DeleteOperation { get; }
        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }

        // this will probably end up in IGraphOperationContext, or a new base shared by delete & merge (but not validate)
        IEnumerable<IGraphDeleteContext> ChildContexts { get; }

        void AddChildContext(IGraphDeleteContext graphDeleteContext);

        Queue<ICommand> Commands { get; }
        void AddCommand(ICommand command);
        void AddCommands(IEnumerable<ICommand> commands);
    }
}
