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

        Task<ContentItem> GetContentItemAsync(IContentManager contentManager, string id);
    }
}
