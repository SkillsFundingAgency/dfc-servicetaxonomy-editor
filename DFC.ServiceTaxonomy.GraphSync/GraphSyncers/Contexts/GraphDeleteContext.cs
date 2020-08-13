using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphDeleteContext : GraphOperationContext, IGraphDeleteItemSyncContext
    {
        //public IDeleteGraphSyncer DeleteGraphSyncer { get; }
        public IDeleteNodeCommand DeleteNodeCommand { get; }
        public DeleteOperation DeleteOperation { get; }
        public IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }
        public IEnumerable<IGraphDeleteContext> ChildContexts => _childContexts;
        public Queue<ICommand> Commands { get; }

        private readonly List<IGraphDeleteContext> _childContexts;

        public GraphDeleteContext(
            ContentItem contentItem,
            IDeleteNodeCommand deleteNodeCommand,
            //IDeleteGraphSyncer deleteGraphSyncer,
            DeleteOperation deleteOperation,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties,
            IGraphDeleteContext? parentGraphDeleteContext)
            : base(contentItem, graphSyncHelper, contentManager, contentItemVersion, parentGraphDeleteContext)
        {
            //DeleteGraphSyncer = deleteGraphSyncer;
            DeleteNodeCommand = deleteNodeCommand;
            DeleteOperation = deleteOperation;
            DeleteIncomingRelationshipsProperties = deleteIncomingRelationshipsProperties;
            Commands = new Queue<ICommand>();

            _childContexts = new List<IGraphDeleteContext>();
        }

        public void AddChildContext(IGraphDeleteContext graphDeleteContext)
        {
            _childContexts.Add(graphDeleteContext);
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
