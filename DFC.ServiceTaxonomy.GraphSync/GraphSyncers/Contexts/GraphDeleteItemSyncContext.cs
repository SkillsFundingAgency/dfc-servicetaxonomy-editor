using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphDeleteItemSyncContext : GraphOperationContext, IGraphDeleteItemSyncContext
    {
        public Queue<ICommand> Commands { get; }

        public GraphDeleteItemSyncContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager)
            : base(contentItem, graphSyncHelper, contentManager)
        {
            Commands = new Queue<ICommand>();
        }

        //todo: replicate pattern for merge
        public void AddCommand(ICommand command)
        {
            Commands.Enqueue(command);
        }

        public void AddCommands(IEnumerable<ICommand> commands)
        {
            foreach (ICommand command in commands)
            {
                Commands.Enqueue(command);
            }
        }
    }
}
