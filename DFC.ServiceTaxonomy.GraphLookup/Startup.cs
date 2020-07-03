using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.GraphLookup.Drivers;
using DFC.ServiceTaxonomy.GraphLookup.GraphSyncers;
using DFC.ServiceTaxonomy.GraphLookup.Handlers;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.GraphLookup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Graph Database
            //services.AddTransient<ILogger, NeoLogger>();
            //todo:
            //services.AddGraphCluster();

            services.AddContentPart<GraphLookupPart>()
                .UseDisplayDriver<GraphLookupPartDisplayDriver>()
                .AddHandler<GraphLookupPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphLookupPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
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

            routes.MapAreaControllerRoute(
                name: "GraphLookup",
                areaName: "DFC.ServiceTaxonomy.GraphLookup",
                pattern: "GraphLookup/SearchLookupNodes",
                defaults: new { controller = "GraphLookupAdmin", action = "SearchLookupNodes" }
            );
        }
    }
}
