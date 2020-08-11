using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    //todo: rename? as deletes and unpublishes
    public interface IDeleteGraphSyncer
    {
        //todo: pass IGraphReplicaSet rather than the name
        Task Delete(ContentItem contentItem, IContentItemVersion contentItemVersion);
        Task Unpublish(ContentItem contentItem, IContentItemVersion contentItemVersion);
        Task DeleteNodesByType(string graphReplicaSetName, string contentType);
    }
}
