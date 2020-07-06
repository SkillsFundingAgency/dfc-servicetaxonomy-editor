using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Wrappers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphMergeContext : IGraphMergeContext
    {
        public IGraphSyncHelper GraphSyncHelper { get; }
        public IGraphReplicaSet GraphReplicaSet { get; }
        public IMergeNodeCommand MergeNodeCommand { get; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }

        public ContentItem ContentItem { get; }
        public IContentManager ContentManager { get; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition? ContentPartFieldDefinition { get; private set; }

        public GraphMergeContext(
            IGraphSyncHelper graphSyncHelper,
            IGraphReplicaSet graphReplicaSet,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentItem contentItem,
            IContentManager contentManager)
        {
            GraphSyncHelper = graphSyncHelper;
            GraphReplicaSet = graphReplicaSet;
            MergeNodeCommand = mergeNodeCommand;
            ReplaceRelationshipsCommand = replaceRelationshipsCommand;
            ContentItem = contentItem;
            ContentManager = contentManager;

            ContentTypePartDefinition = default!;
        }

        public void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition)
        {
            ContentPartFieldDefinition = contentPartFieldDefinition != null
                ? new ContentPartFieldDefinitionWrapper(contentPartFieldDefinition) : default;
        }
    }
}
