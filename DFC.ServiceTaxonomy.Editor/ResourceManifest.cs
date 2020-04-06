using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Editor
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("ncs")
                .SetUrl("~/DFC.ServiceTaxonomy.Editor/ncs.js");
        }
    }
}
