using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    //todo: better name(s)
    public interface IContentItemVersion
    {
        VersionOptions VersionOptions { get; }
        (bool? latest, bool? published) ContentItemIndexFilterTerms { get; }
        string GraphReplicaSetName { get; }
        string ContentApiBaseUrl { get; }

        Task<ContentItem> GetContentItem(IContentManager contentManager, string id);
    }
}
