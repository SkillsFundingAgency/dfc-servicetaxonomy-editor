using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.ContentItemVersions
{
    public class PublishedContentItemVersion : ContentItemVersion, IPublishedContentItemVersion
    {
        public PublishedContentItemVersion(IConfiguration configuration)
            : base(DataSyncReplicaSetNames.Published,
                VersionOptions.Published,
                (null, true),
                GetContentApiBaseUrlFromConfig(configuration, "ContentApiPrefix"))
        {
        }
    }
}
