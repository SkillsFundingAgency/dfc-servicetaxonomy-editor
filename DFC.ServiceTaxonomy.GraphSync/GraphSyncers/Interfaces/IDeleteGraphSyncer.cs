using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    //todo: rename? as deletes and unpublishes
    public interface IDeleteGraphSyncer
    {
        Task<IAllowSyncResult> DeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            //string operation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null, // put into context
            IGraphDeleteContext? parentContext = null);

        //todo: pass IGraphReplicaSet rather than the name
        Task Delete();
        Task Unpublish();
        Task DeleteNodesByType(string graphReplicaSetName, string contentType);
    }
}
