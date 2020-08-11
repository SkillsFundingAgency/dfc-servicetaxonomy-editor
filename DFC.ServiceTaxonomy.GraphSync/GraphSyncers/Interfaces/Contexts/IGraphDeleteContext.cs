using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphDeleteContext : IGraphOperationContext
    {
        void AddCommand(ICommand command);
        void AddCommands(IEnumerable<ICommand> commands);
    }
}
