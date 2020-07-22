using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IMergeGraphSyncer
    {
        Task<IMergeNodeCommand?> SyncToGraphReplicaSet(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null);
    }
}
