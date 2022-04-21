using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Services;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces
{
    //todo: rename? as deletes and unpublishes
    public interface IDeleteDataSyncer
    {
        string? DataSyncReplicaSetName { get; }

        Task<IAllowSync> DeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null,
            IDataDeleteContext? parentContext = null);

        Task<IAllowSync> DeleteIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null);

        Task Delete();

        Task DeleteEmbedded(ContentItem contentItem);
    }
}
