using System;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.GraphSync.Drivers;
using DFC.ServiceTaxonomy.GraphSync.Handlers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using Microsoft.Extensions.Configuration;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.GraphSync
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<NamespacePrefixConfiguration>(configuration.GetSection("GraphSync"));

            services.AddContentPart<GraphSyncPart>();
            services.AddScoped<IContentPartDisplayDriver, GraphSyncPartDisplayDriver>();
            //services.AddScoped<IContentPartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartHandler, GraphSyncPartHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.GraphSync",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
