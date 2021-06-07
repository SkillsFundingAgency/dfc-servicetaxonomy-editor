using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.PageLocation
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;
#pragma warning disable S3963 // "static" fields should be initialized inline
        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("pagelocations")
                .SetUrl("~/DFC.ServiceTaxonomy.PageLocation/Scripts/pagelocations.js")
                .SetVersion("1.0.0");
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
