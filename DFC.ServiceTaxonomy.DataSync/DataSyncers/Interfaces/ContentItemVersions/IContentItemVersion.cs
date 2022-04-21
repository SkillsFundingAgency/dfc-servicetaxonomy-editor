using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions
{
    //todo: better name(s)
    public interface IContentItemVersion
    {
        VersionOptions VersionOptions { get; }
        (bool? latest, bool? published) ContentItemIndexFilterTerms { get; }
        string DataSyncReplicaSetName { get; }
        string ContentApiBaseUrl { get; }

        Task<ContentItem?> GetContentItem(IContentManager contentManager, string id);
    }
}
