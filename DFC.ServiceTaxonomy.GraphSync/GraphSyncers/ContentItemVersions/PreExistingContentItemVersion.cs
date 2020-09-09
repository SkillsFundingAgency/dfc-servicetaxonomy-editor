using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ContentItemVersions
{
    public class PreExistingContentItemVersion : IPreExistingContentItemVersion
    {
        public PreExistingContentItemVersion()
        {
            ContentApiBaseUrl = string.Empty;
        }

        public void SetContentApiBaseUrl(string? contentApiBaseUrl)
        {
            ContentApiBaseUrl = contentApiBaseUrl ?? string.Empty;
        }

        public string ContentApiBaseUrl { get; set; }
        public VersionOptions VersionOptions => throw new NotImplementedException();
        public (bool? latest, bool? published) ContentItemIndexFilterTerms => throw new NotImplementedException();
        public string GraphReplicaSetName => throw new NotImplementedException();
        public Task<ContentItem?> GetContentItem(IContentManager contentManager, string id) => throw new NotImplementedException();
    }
}
