using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Services;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    //todo: rename? as deletes and unpublishes
    public interface IDeleteGraphSyncer
    {
        string? GraphReplicaSetName { get; }

        Task<IAllowSyncResult> DeleteAllowed(ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null,
            IGraphDeleteContext? parentContext = null);

        Task<IAllowSyncResult> DeleteIfAllowed(ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null);

        Task Delete();

        Task DeleteEmbedded(ContentItem contentItem);
    }
}
