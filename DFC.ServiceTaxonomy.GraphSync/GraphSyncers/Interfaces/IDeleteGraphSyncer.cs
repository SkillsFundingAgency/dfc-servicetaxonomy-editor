using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IDeleteGraphSyncer
    {
        Task DeleteFromGraph(ContentItem contentItem);
        Task DeleteNodesByType(string contentType);
        Task<bool> VerifyDeletion(ContentItem contentItem);
    }
}
