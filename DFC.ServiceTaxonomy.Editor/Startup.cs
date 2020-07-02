using DFC.ServiceTaxonomy.Editor.Security;
using DFC.ServiceTaxonomy.Events.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddOrchardCms();

            services.AddEventGridPublishing(Configuration);

            //todo: extension method in neo project
            services.Configure<Neo4jConfiguration>(Configuration.GetSection("Neo4j"));
            services.Configure<GraphSyncPartSettingsConfiguration>(Configuration.GetSection(nameof(GraphSyncPartSettings)));
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // UseSecurityHeaders must come before UseOrchardCore
            app.UseSecurityHeaders()
                .UseOrchardCore();
        }
    }
}
