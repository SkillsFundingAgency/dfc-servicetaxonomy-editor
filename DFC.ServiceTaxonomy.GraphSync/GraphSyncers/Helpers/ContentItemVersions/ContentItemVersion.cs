using System.Configuration;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.ContentItemVersions
{
    public class ContentItemVersion : IContentItemVersion
    {
        protected ContentItemVersion(
            string graphReplicaSetName,
            VersionOptions versionOptions,
            (bool? latest, bool? published) contentItemIndexFilterTerms,
            string contentApiBaseUrl)
        {
            GraphReplicaSetName = graphReplicaSetName;
            VersionOptions = versionOptions;
            ContentItemIndexFilterTerms = contentItemIndexFilterTerms;
            ContentApiBaseUrl = contentApiBaseUrl;
        }

        public string GraphReplicaSetName { get; }
        public VersionOptions VersionOptions { get; }

        // ContentItem properties according to item version
        //  Latest  Published
        //     1        0        draft only
        //     1        1        published only
        //     0        1        published (with separate draft version)
        //     1        0        draft (with separate published version)
        //     0        0        old (replaced) published version
        //     1        1        new (replacement) published version
        //     <Not Kept>        old (replaced) draft version
        //     1        0        new (replacement) draft version
        public (bool? latest, bool? published) ContentItemIndexFilterTerms { get; }

        //todo: static Published and Draft ContentItemVersions?
        // with GraphReplicaSetName property, used in ToString?
        // public override string ToString()
        // {
        //     return
        // }

        //todo: the structure of this class?
        public async Task<ContentItem> GetContentItemAsync(IContentManager contentManager, string id)
        {
            ContentItem? contentItem = null;
            if (GraphReplicaSetName == GraphReplicaSetNames.Preview)
                contentItem = await contentManager.GetAsync(id, VersionOptions.Draft);

            return contentItem ?? await contentManager.GetAsync(id, VersionOptions.Published);
        }

        // ?
        // public IGraphReplicaSet GraphReplicaSet(IGraphCluster graphCluster)
        // {
        //     return graphCluster.GetGraphReplicaSet(GraphReplicaSetName);
        // }

        public string ContentApiBaseUrl { get; }

        protected static string GetContentApiBaseUrlFromConfig(IConfiguration configuration, string contentApiPrefixConfigName)
        {
            return configuration.GetValue<string?>(contentApiPrefixConfigName)
                       ?.ToLowerInvariant()
                   ?? throw new ConfigurationErrorsException(
                       $"{contentApiPrefixConfigName} not in config.");
        }
    }
}
