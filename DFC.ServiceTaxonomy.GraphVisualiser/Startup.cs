using System;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using DFC.ServiceTaxonomy.Neo4j.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Helpers.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.GraphVisualiser
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<OwlDataGeneratorConfigModel>(configuration.GetSection(nameof(OwlDataGeneratorConfigModel)));
            services.AddSingleton<ITcpPortHelper, TcpPortHelper>();
            services.AddSingleton<IGraphDatabase, NeoGraphDatabase>();
            services.AddTransient<INeo4JToOwlGeneratorService, Neo4JToOwlGeneratorService>();
            services.AddTransient<IOrchardToOwlGeneratorService, OrchardToOwlGeneratorService>();
            services.AddScoped<INavigationProvider, AdminMenuService>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseStaticFiles();

            routes.MapAreaControllerRoute(
                name: "Visualise",
                areaName: typeof(DFC.ServiceTaxonomy.GraphVisualiser.Startup).Namespace,
                pattern: $"Visualise/{nameof(Controllers.VisualiseController.Data)}",
                defaults: new { controller = "Visualise", action = nameof(Controllers.VisualiseController.Data) }
            );
            routes.MapAreaControllerRoute(
                name: "WebVOWL",
                areaName: typeof(DFC.ServiceTaxonomy.GraphVisualiser.Startup).Namespace,
                pattern: $"Visualise/{nameof(Controllers.VisualiseController.Viewer)}",
                defaults: new { controller = "Visualise", action = nameof(Controllers.VisualiseController.Viewer) }
            );
        }
    }
}
