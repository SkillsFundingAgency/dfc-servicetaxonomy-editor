using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    //todo: merge (partly?) with graphsynchelper??
    public interface IGraphMergeContext : IGraphOperationContext
    {
        ContentItem ContentItem { get; }
        IContentManager ContentManager { get; }
        IContentItemVersion ContentItemVersion { get; }
        ContentTypePartDefinition ContentTypePartDefinition { get; }
        IContentPartFieldDefinition? ContentPartFieldDefinition  { get; }

        IGraphSyncHelper GraphSyncHelper { get; }
        public IGraphReplicaSet GraphReplicaSet { get; }
        IMergeNodeCommand MergeNodeCommand { get; }
        IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }

        IGraphMergeContext? ParentGraphMergeContext { get; }

        void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition);
    }
}
