using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.ContentItemVersions
{
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
}
