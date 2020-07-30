using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IMergeGraphSyncer
    {
        //todo: tidy these up (by removing access)
        public IMergeNodeCommand MergeNodeCommand { get; }
        IGraphMergeContext? GraphMergeContext { get; }

        Task<IAllowSyncResult> SyncToGraphReplicaSetIfAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null);

        Task<IAllowSyncResult> SyncAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null);

        Task<IMergeNodeCommand?> SyncToGraphReplicaSet();
    }
}
