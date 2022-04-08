using System;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using DFC.ServiceTaxonomy.GraphVisualiser.Drivers;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.GraphVisualiser
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // configuration
            //todo: extension method & move to editor startup
            services.Configure<OwlDataGeneratorConfigModel>(_configuration.GetSection(nameof(OwlDataGeneratorConfigModel)));

            //todo:??
            //services.AddGraphCluster();
            services.AddTransient<INeo4JToOwlGeneratorService, Neo4JToOwlGeneratorService>();
            services.AddTransient<IOrchardToOwlGeneratorService, OrchardToOwlGeneratorService>();
            services.AddScoped<INavigationProvider, AdminMenuService>();
            services.AddScoped<IContentDisplayDriver, ContentVisualiseDriver>();
            services.AddSingleton<IIndexProvider, GraphSyncPartIndexProvider>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseStaticFiles();

#pragma warning disable CS8604 // Possible null reference argument.
            routes.MapAreaControllerRoute(
                name: "Visualise",
                areaName: typeof(Startup).Namespace,
                pattern: $"Visualise/{nameof(Controllers.VisualiseController.Data)}",
                defaults: new { controller = "Visualise", action = nameof(Controllers.VisualiseController.Data) }
            );
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
            routes.MapAreaControllerRoute(
                name: "WebVOWL",
                areaName: typeof(Startup).Namespace,
                pattern: $"Visualise/{nameof(Controllers.VisualiseController.Viewer)}",
                defaults: new { controller = "Visualise", action = nameof(Controllers.VisualiseController.Viewer) }
            );
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
            routes.MapAreaControllerRoute(
                name: "VisualiserRedirect",
                areaName: typeof(Startup).Namespace,
                pattern: $"Visualise/{nameof(Controllers.VisualiseController.NodeLink)}",
                defaults: new { controller = "Visualise", action = nameof(Controllers.VisualiseController.NodeLink) }
            );
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
