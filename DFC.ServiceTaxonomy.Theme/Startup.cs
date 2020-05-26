﻿using DFC.ServiceTaxonomy.Theme.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Infrastructure.Html;
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

            services.Configure<HtmlSanitizerOptions>(o =>
            {
                o.Configure = sanitizer =>
                {
                    //todo: presumably sanitizer removes, rather than replaces
                    // only want to sanitize local urls, although out csp policy will block external urls anyway
                    sanitizer.AllowedSchemes.Remove("http");
                    sanitizer.AllowedAttributes.Add("class");
                };
            });
        }
    }
}
