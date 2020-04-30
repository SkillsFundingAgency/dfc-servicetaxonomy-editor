using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Theme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        //todo: do we need an admin theme based of TheAdmin so we can add this to the layout?
        //or do we override the trumbowyg Edit view? problem there is keeping up with changes
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("StaxTheme-trumbowyg-scoped-govuk-frontend")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/wwwroot/Styles/trumbowyg-scoped-govuk-frontend.min.css", "~/DFC.ServiceTaxonomy.Theme/wwwroot/Styles/trumbowyg-scoped-govuk-frontend.css")
                .SetVersion("1.0.0");
        }
    }
}
