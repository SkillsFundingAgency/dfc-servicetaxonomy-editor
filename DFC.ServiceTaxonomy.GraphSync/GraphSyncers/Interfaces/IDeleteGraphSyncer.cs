using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IDeleteGraphSyncer
    {
        //todo: pass IGraphReplicaSet rather than the name
        Task DeleteFromGraphReplicaSet(string graphReplicaSetName, ContentItem contentItem);
        Task DeleteNodesByType(string graphReplicaSetName, string contentType);
    }
}
