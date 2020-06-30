using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IDeleteGraphSyncer
    {
        Task DeleteFromGraphReplicaSet(string graphReplicaSetName, ContentItem contentItem);
        Task DeleteNodesByType(string graphReplicaSetName, string contentType);
    }
}
