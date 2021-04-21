using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Theme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

#pragma warning disable S3963 // "static" fields should be initialized inline
        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheBlogTheme-vendor-bootstrap-bundle")
                .SetDependencies("TheBlogTheme-vendor-jQuery")
                .SetUrl("~/TheBlogTheme/vendor/bootstrap/js/bootstrap.bundle.min.js", "~/TheBlogTheme/vendor/bootstrap/js/bootstrap.bundle.js")
#pragma warning disable S1075 // URIs should not be hardcoded
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.bundle.js")
#pragma warning restore S1075 // URIs should not be hardcoded
                .SetCdnIntegrity("sha384-ho+j7jyWK8fNQe+A12Hb8AhRq26LrZ/JpcUGGOn+Y7RsweNrtN/tE3MoK7ZeZDyx", "sha384-q/QDYob/o4XN/fpFWt7AEU+hWQLAc0FCviSzMpKqarlw2VVqk2mgn971KCJ64zpo")
                .SetVersion("4.5.3");

            _manifest
                .DefineStyle("TheBlogTheme-vendor-bootstrap")
                .SetUrl("~/TheBlogTheme/vendor/bootstrap/css/bootstrap.min.css", "~/TheBlogTheme/vendor/bootstrap/css/bootstrap.css")
#pragma warning disable S1075 // URIs should not be hardcoded
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.css")
#pragma warning restore S1075 // URIs should not be hardcoded
                .SetCdnIntegrity("sha384-TX8t27EcRE3e/ihU7zmQxVncDAy5uIKz4rEkgIXeMed4M0jlfIDPvg6uqKI2xXr2", "sha384-Ro2DNoUODgrLmRM7WL/mbXZ1D6WaudEiPPceIZTfzZrTahyJAxLMj5TF2RQwrpiG")
                .SetVersion("4.5.3");


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
