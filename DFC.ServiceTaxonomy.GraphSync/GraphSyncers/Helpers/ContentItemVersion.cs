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
        ContentItemVersion Published { get; }
        ContentItemVersion Preview { get; }

        ContentItemVersion Get(string graphReplicaSetName);
    }

    public class ContentItemVersionFactory : IContentItemVersionFactory
    {
        private readonly IConfiguration _configuration;
        private static ContentItemVersion? _published;
        private static ContentItemVersion? _preview;
        private static readonly object _publishedLock = new object();
        private static readonly object _previewLock = new object();

        //todo: inject (deferred ction) singletons from services
        #pragma warning disable S2696
        public ContentItemVersion Published
        {
            get
            {
                lock (_publishedLock)
                {
                    if (_published == null)
                    {
                        string contentApiBaseUrl = _configuration.GetValue<string>("ContentApiPrefix")
                                                       ?.ToLowerInvariant()
                                                   ?? throw new ConfigurationErrorsException(
                                                       "ContentApiPrefix not in config.");

                        _published = new ContentItemVersion(
                            GraphReplicaSetNames.Published,
                            VersionOptions.Published,
                            (null, true),
                            contentApiBaseUrl);
                    }
                }

                return _published;

            }
        }

        public ContentItemVersion Preview
        {
            get
            {
                lock (_previewLock)
                {
                    if (_preview == null)
                    {
                        string contentApiBaseUrl = _configuration.GetValue<string>("PreviewContentApiPrefix")
                                                       ?.ToLowerInvariant()
                                                   ?? throw new ConfigurationErrorsException(
                                                       "PreviewContentApiPrefix not in config.");

                        _preview = new ContentItemVersion(
                            GraphReplicaSetNames.Published,
                            VersionOptions.Published,
                            (null, true),
                            contentApiBaseUrl);
                    }
                }

                return _preview;
            }
        }
        #pragma warning restore S2696

        public ContentItemVersionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ContentItemVersion Get(string graphReplicaSetName)
        {
            switch (graphReplicaSetName)
            {
                case GraphReplicaSetNames.Published:
                    return Published;
                case GraphReplicaSetNames.Preview:
                    return Preview;
                default:
                    throw new GraphSyncException($"Unknown graph replica set '{graphReplicaSetName}'.");
            }
        }
    }

    public class ContentItemVersion : IContentItemVersion
    {
        //todo: private ctor that takes 3 args?
        // public static ContentItemVersion Published => new ContentItemVersion(
        //     GraphReplicaSetNames.Published,
        //     VersionOptions.Published,
        //     (null, true));
        // public static ContentItemVersion Preview => new ContentItemVersion(
        //     GraphReplicaSetNames.Preview,
        //     VersionOptions.Draft,
        //     (true, null));

        //todo:  make private, pass VersionOptions, ContentItemIndexFilterTerms
        public ContentItemVersion(
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
    }
}
