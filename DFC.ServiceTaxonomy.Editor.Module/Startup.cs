using System;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.Editor.Module
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            //todo: are these only necessary for fluid?
            // TemplateContext.GlobalMemberAccessStrategy.Register<GraphUriIdField>();
            // TemplateContext.GlobalMemberAccessStrategy.Register<DisplayGraphUriIdFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            //
            services.Configure<Neo4jConfiguration>(configuration.GetSection("Neo4j"));
            // services.AddSingleton<IGraphDatabase, NeoGraphDatabase>();
            services.AddActivity<SyncToGraphTask, SyncToGraphTaskDisplay>();

            // services.Configure<NamespacePrefixConfiguration>(configuration.GetSection("GraphUriIdField"));
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.Editor.Module",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
