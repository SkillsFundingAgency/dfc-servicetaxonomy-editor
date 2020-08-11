using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ContentItemVersions
{
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
}
