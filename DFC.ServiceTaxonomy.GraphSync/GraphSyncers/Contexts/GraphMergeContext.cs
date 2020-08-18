using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
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
        public IEnumerable<IReplaceRelationshipsCommand>? RecreateIncomingPreviewContentPickerRelationshipsCommands { get; set; }

        public GraphMergeContext(
            IMergeGraphSyncer mergeGraphSyncer,
            IGraphSyncHelper graphSyncHelper,
            IGraphReplicaSet graphReplicaSet,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersionFactory contentItemVersionFactory,
            IGraphMergeContext? parentGraphMergeContext)
        : base(contentItem, graphSyncHelper, contentManager, contentItemVersionFactory.Get(graphReplicaSet.Name), parentGraphMergeContext)
        {
            MergeGraphSyncer = mergeGraphSyncer;
            GraphReplicaSet = graphReplicaSet;
            MergeNodeCommand = mergeNodeCommand;
            ReplaceRelationshipsCommand = replaceRelationshipsCommand;

            RecreateIncomingPreviewContentPickerRelationshipsCommands = null;
        }
    }
}
