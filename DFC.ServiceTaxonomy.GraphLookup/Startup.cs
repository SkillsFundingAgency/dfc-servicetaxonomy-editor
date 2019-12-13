using System;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.GraphLookup.Drivers;
using DFC.ServiceTaxonomy.GraphLookup.GraphSyncers;
using DFC.ServiceTaxonomy.GraphLookup.Handlers;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Configuration;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.GraphLookup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<Neo4jConfiguration>(configuration.GetSection("Neo4j"));
            services.AddSingleton<IGraphDatabase, NeoGraphDatabase>();

            services.AddContentPart<GraphLookupPart>();
            services.AddScoped<IContentPartDisplayDriver, GraphLookupPartDisplayDriver>();
//            services.AddScoped<IContentPartDefinitionDisplayDriver, GraphLookupPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphLookupPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartHandler, GraphLookupPartHandler>();
            services.AddScoped<IContentPartGraphSyncer, GraphLookupPartGraphSyncer>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.GraphLookup",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
