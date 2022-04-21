using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.ContentItemVersions
{
    public class PreviewContentItemVersion : ContentItemVersion, IPreviewContentItemVersion
    {
        public PreviewContentItemVersion(IConfiguration configuration)
            : base(DataSyncReplicaSetNames.Preview,
                VersionOptions.Draft,
                (true, null),
                GetContentApiBaseUrlFromConfig(configuration, "PreviewContentApiPrefix"))
        {
        }
    }
}
