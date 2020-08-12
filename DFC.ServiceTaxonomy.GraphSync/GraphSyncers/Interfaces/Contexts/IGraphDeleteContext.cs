using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphDeleteContext : IGraphOperationContext
    {
        DeleteOperation DeleteOperation { get; }
        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }

        Queue<ICommand> Commands { get; }
        void AddCommand(ICommand command);
        void AddCommands(IEnumerable<ICommand> commands);
    }
}
