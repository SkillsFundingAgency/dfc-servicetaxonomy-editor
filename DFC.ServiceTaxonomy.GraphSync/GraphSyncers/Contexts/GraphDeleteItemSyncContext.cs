using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphDeleteItemSyncContext : GraphOperationContext, IGraphDeleteItemSyncContext
    {
        private readonly Queue<ICommand> _commands;

        public GraphDeleteItemSyncContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager)
            : base(contentItem, graphSyncHelper, contentManager)
        {
            _commands = new Queue<ICommand>();
        }

        //todo: replicate pattern for merge
        public void AddCommand(ICommand command)
        {
            _commands.Enqueue(command);
        }

        public void AddCommands(IEnumerable<ICommand> commands)
        {
            foreach (ICommand command in commands)
            {
                _commands.Enqueue(command);
            }
        }
    }
}
