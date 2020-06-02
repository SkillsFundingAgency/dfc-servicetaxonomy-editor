using DFC.ServiceTaxonomy.Theme.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Theme
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            //todo: use extension?
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(ResourceInjectionFilter));
            });

            services.ConfigureHtmlSanitizer(sanitizer =>
            {
                //todo: presumably sanitizer removes, rather than replaces
                // only want to sanitize local urls, although our csp policy will block external urls anyway
                sanitizer.AllowedSchemes.Remove("http");
                sanitizer.AllowedAttributes.Add("class");
            });
        }
    }
}
