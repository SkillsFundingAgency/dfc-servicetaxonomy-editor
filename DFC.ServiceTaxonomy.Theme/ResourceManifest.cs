using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Theme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        //todo: do we need an admin theme based of TheAdmin so we can add this to the layout?
        //or do we override the trumbowyg Edit view? problem there is keeping up with changes
        // plug in the original as a partial?? can it be set to base?
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            //todo: cache-busting
            manifest
                .DefineStyle("StaxTheme-trumbowyg-scoped-govuk-frontend")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Styles/trumbowyg_scoped_govuk_frontend.min.css", "~/DFC.ServiceTaxonomy.Theme/Styles/trumbowyg_scoped_govuk_frontend.css")
                .SetVersion("1.0.0");

            manifest
                .DefineScript("StaxTheme-trumbowyg-gds")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Scripts/trumbowyg.gds.js")
                .SetVersion("1.0.0");

            manifest
                .DefineScript("ncs")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Scripts/ncs.js")
                .SetVersion("1.0.0");
        }
    }
}
