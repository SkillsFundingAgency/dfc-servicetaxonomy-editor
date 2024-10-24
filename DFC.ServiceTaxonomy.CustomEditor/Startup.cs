using System;
using DFC.ServiceTaxonomy.CustomEditor.Drivers;
using DFC.ServiceTaxonomy.CustomEditor.Handlers;
using DFC.ServiceTaxonomy.CustomEditor.Shapes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Services;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.CustomEditor
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeTableProvider, SummaryAdminShapes>();
            services.AddScoped<IContentDisplayDriver, CustomContentDriver>();
            services.AddScoped<IAuditTrailEventHandler, CustomAuditTrailEventHandler>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.CustomEditor",
                pattern: "StaxPreview/GoToUrl",
                defaults: new { controller = "StaxPreview", action = "GoToUrl" });
        }
    }
}
