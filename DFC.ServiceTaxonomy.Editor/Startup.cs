using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Content.Configuration;
using DFC.ServiceTaxonomy.Editor.Security;
using DfE.NCS.Framework.Core.Crypto;
using DfE.NCS.Framework.Core.Crypto.Interfaces;
using DfE.NCS.Framework.Event.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Media;

namespace DFC.ServiceTaxonomy.Editor
{
    public class Startup
    {
        public const string ConfigAppSettings = "Configuration";

        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(options =>
                options.ConnectionString = Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddMemoryCache();

            services.AddOrchardCms()
                .ConfigureServices(se => se.ConfigureHtmlSanitizer((sanitizer) =>
                {
                    sanitizer.AllowDataAttributes = true;
                    sanitizer.AllowedAttributes.Add("id");
                    sanitizer.AllowedAttributes.Add("aria-labelledby");
                    sanitizer.AllowedTags.Add("iframe");
                    sanitizer.AllowedTags.Add("svg");
                    sanitizer.AllowedTags.Add("path");
                    sanitizer.AllowedTags.Add("form");
                    sanitizer.AllowedAttributes.Add("fill");
                    sanitizer.AllowedAttributes.Add("d");
                    sanitizer.AllowedAttributes.Add("xmlns");
                    sanitizer.AllowedAttributes.Add("viewBox");
                    sanitizer.AllowedAttributes.Add("allowfullscreen");
                    sanitizer.AllowedSchemes.Add("mailto");
                    sanitizer.AllowedSchemes.Add("tel");

                    sanitizer.AllowedAttributes.Remove("style");
                }));

            services.Configure<CookiePolicyOptions>(options => options.Secure = CookieSecurePolicy.Always);

            services.AddOrchardCore()
                .ConfigureServices(s =>
                {
                    s.ConfigureApplicationCookie(options => options.Cookie.Name = "stax_Default");
                    s.AddAntiforgery(options => options.Cookie.Name = "staxantiforgery_Default");
                }, order: 10);

            services.Configure<AzureAdSettings>(Configuration.GetSection("AzureAdSettings"));

            services.AddNcsEventGridServices(Configuration);
            services.AddScoped<ICryptographyManager, CryptographyManager>();

            services.PostConfigure(SetupMediaConfig());
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
            app.UseCookiePolicy();
            // UseSecurityHeaders must come before UseOrchardCore
            app.UsePoweredByOrchardCore(false);
            app.UseSecurityHeaders(Configuration)
                .UseOrchardCore();
        }

        private Action<MediaOptions> SetupMediaConfig() =>
            o =>
            {
                o.AllowedFileExtensions = new HashSet<string>
                {
                    ".jpg",
                    ".png",
                    ".gif",
                    ".ico",
                    ".svg"
                };
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                o.CdnBaseUrl = Configuration.GetValue<string>(Constants.Common.DigitalAssetsCdnKey).TrimEnd('/');
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            };
    }
}
