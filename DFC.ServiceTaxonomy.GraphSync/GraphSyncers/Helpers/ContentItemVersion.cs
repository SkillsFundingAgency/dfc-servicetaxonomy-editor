using System.Configuration;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public interface IContentItemVersionFactory
    {
        IContentItemVersion Get(string graphReplicaSetName);
    }

    // workaround for di not supporting names
    public class ContentItemVersionFactory : IContentItemVersionFactory
    {
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;

        public ContentItemVersionFactory(
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion)
        {
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
        }

        public IContentItemVersion Get(string graphReplicaSetName)
        {
            return graphReplicaSetName switch
            {
                GraphReplicaSetNames.Published => _publishedContentItemVersion,
                GraphReplicaSetNames.Preview => _previewContentItemVersion,
                _ => throw new GraphSyncException($"Unknown graph replica set '{graphReplicaSetName}'.")
            };
        }
    }

    public interface IPublishedContentItemVersion : IContentItemVersion
    {
    }

    public class PublishedContentItemVersion : ContentItemVersion, IPublishedContentItemVersion
    {
        public PublishedContentItemVersion(IConfiguration configuration)
            : base(GraphReplicaSetNames.Published,
                VersionOptions.Published,
                (null, true),
                GetContentApiBaseUrlFromConfig(configuration, "ContentApiPrefix"))
        {
        }
    }

    public interface IPreviewContentItemVersion : IContentItemVersion
    {
    }

    public class PreviewContentItemVersion : ContentItemVersion, IPreviewContentItemVersion
    {
        public PreviewContentItemVersion(IConfiguration configuration)
            : base(GraphReplicaSetNames.Preview,
                VersionOptions.Draft,
                (true, null),
                GetContentApiBaseUrlFromConfig(configuration, "PreviewContentApiPrefix"))
        {
        }
    }

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
