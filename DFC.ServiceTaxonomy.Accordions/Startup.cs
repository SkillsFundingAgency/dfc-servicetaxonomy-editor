using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.Accordions.Drivers;
using DFC.ServiceTaxonomy.Accordions.Handlers;
using DFC.ServiceTaxonomy.Accordions.Models;
using DFC.ServiceTaxonomy.Accordions.Settings;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.Accordions
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddContentPart<AccordionPart>()
                .UseDisplayDriver<AccordionPartDisplayDriver>()
                .AddHandler<AccordionPartHandler>();

            services.AddScoped<IContentPartDefinitionDisplayDriver, AccordionPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.Accordions",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
