using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.PageLocation
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("pagelocations")
                .SetUrl("~/DFC.ServiceTaxonomy.PageLocation/Scripts/pagelocations.js")
                .SetVersion("1.0.0");
        }
    }
}
