using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.ContentItemVersions
{
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

        // we _could_ return NeutralContentItemVersion if null
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
}
