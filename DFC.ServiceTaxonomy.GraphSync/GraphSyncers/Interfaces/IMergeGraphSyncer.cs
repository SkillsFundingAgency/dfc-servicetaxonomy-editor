using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IMergeGraphSyncer
    {
        Task<IMergeNodeCommand?> SyncToGraphReplicaSet(
            string graphReplicaSetName,
            ContentItem contentItem,
            IContentManager contentManager);
    }
}
