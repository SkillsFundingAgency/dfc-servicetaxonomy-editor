using DFC.ServiceTaxonomy.Editor.Security;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using DFC.ServiceTaxonomy.Editor.Configuration;

namespace DFC.ServiceTaxonomy.Editor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(options =>
                options.InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddOrchardCms().ConfigureServices(se => se.ConfigureHtmlSanitizer((sanitizer) =>
            {
                sanitizer.AllowDataAttributes = true;
                sanitizer.AllowedAttributes.Add("id");
                sanitizer.AllowedAttributes.Add("aria-labelledby");
            }));

            //todo: do this in each library??? if so, make sure it doesn't add services or config twice
            services.AddGraphCluster(options =>
                Configuration.GetSection(Neo4jOptions.Neo4j).Bind(options));

            services.Configure<GraphSyncPartSettingsConfiguration>(Configuration.GetSection(nameof(GraphSyncPartSettings)));
            services.Configure<CookiePolicyOptions>(options =>
            {
               options.Secure = CookieSecurePolicy.Always;
            });
            services.AddEventGridPublishing(Configuration);
            services.AddOrchardCore().ConfigureServices(s => s.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "stax_Default";
            }), order:10);

            services.AddOrchardCore().ConfigureServices(s => s.AddAntiforgery(options =>
            {
                options.Cookie.Name = "staxantiforgery_Default";
            }), order:10);

            services.Configure<PagesConfiguration>(Configuration.GetSection("Pages"));
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookiePolicy();
            // UseSecurityHeaders must come before UseOrchardCore
            app.UsePoweredByOrchardCore(false);
            app.UseSecurityHeaders()
                .UseOrchardCore();
        }
    }
}
