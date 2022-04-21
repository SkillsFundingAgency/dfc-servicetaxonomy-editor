using DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.ContentItemVersions
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
        public IContentItemVersion Get(string dataSyncReplicaSetName)
        {
            return dataSyncReplicaSetName switch
            {
                DataSyncReplicaSetNames.Published => _publishedContentItemVersion,
                DataSyncReplicaSetNames.Preview => _previewContentItemVersion,
                _ => throw new DataSyncException($"Unknown data sync replica set '{dataSyncReplicaSetName}'.")
            };
        }
    }
}
