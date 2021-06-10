using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Theme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        //todo: do we need an admin theme based of TheAdmin so we can add this to the layout?
        //or do we override the trumbowyg Edit view? problem there is keeping up with changes
        // plug in the original as a partial?? can it be set to base?
        private static ResourceManifest _manifest;
#pragma warning disable S3963 // "static" fields should be initialized inline
        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineStyle("StaxTheme-trumbowyg-scoped-govuk-frontend")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Styles/trumbowyg_scoped_govuk_frontend.min.css", "~/DFC.ServiceTaxonomy.Theme/Styles/trumbowyg_scoped_govuk_frontend.css")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("StaxTheme-trumbowyg-gds")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Scripts/trumbowyg.gds.js")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("ncs")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Scripts/ncs.js")
                .SetVersion("1.0.0");

            _manifest
                .DefineStyle("StaxTheme-unpublish-later")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Styles/unpublish-later.min.css", "~/DFC.ServiceTaxonomy.Theme/Styles/unpublish-later.css")
                .SetVersion("1.0.0");

            _manifest
                .DefineStyle("StaxTheme-content-approval")
                .SetUrl("~/DFC.ServiceTaxonomy.Theme/Styles/content-approval.min.css", "~/DFC.ServiceTaxonomy.Theme/Styles/content-approval.css")
                .SetVersion("1.0.0");
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
