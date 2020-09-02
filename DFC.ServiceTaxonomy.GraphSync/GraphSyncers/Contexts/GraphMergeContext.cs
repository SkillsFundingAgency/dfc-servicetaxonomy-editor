using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphMergeContext : GraphSyncContext, IGraphMergeItemSyncContext
    {
        public new IGraphMergeContext? ParentContext { get; }
        public new IEnumerable<IGraphMergeContext> ChildContexts => _childContexts.Cast<IGraphMergeContext>();

        public IMergeGraphSyncer MergeGraphSyncer { get; }
        public IGraphReplicaSet GraphReplicaSet { get; }
        public IMergeNodeCommand MergeNodeCommand { get; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }
        public List<ICommand> ExtraCommands { get; }

        public GraphMergeContext(
            IMergeGraphSyncer mergeGraphSyncer,
            ISyncNameProvider syncNameProvider,
            IGraphReplicaSet graphReplicaSet,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersionFactory contentItemVersionFactory,
            IGraphMergeContext? parentGraphMergeContext,
            IServiceProvider serviceProvider)
            : base(
                contentItem,
                syncNameProvider,
                contentManager,
                contentItemVersionFactory.Get(graphReplicaSet.Name),
                parentGraphMergeContext,
                serviceProvider.GetRequiredService<ILogger<GraphMergeContext>>())
        {
            MergeGraphSyncer = mergeGraphSyncer;
            GraphReplicaSet = graphReplicaSet;
            MergeNodeCommand = mergeNodeCommand;
            ReplaceRelationshipsCommand = replaceRelationshipsCommand;

            ExtraCommands = new List<ICommand>();
        }
    }
}
