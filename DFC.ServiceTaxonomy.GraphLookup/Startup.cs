using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.GraphLookup.Drivers;
using DFC.ServiceTaxonomy.GraphLookup.Handlers;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.GraphLookup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<GraphLookupPart>();
            services.AddScoped<IContentPartDisplayDriver, GraphLookupPartDisplayDriver>();
            services.AddScoped<IContentPartDefinitionDisplayDriver, GraphLookupPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartHandler, GraphLookupPartHandler>();
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