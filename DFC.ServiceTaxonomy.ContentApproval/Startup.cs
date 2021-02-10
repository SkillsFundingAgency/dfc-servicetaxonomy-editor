using System;
using DFC.ServiceTaxonomy.ContentApproval.Drivers;
using DFC.ServiceTaxonomy.ContentApproval.Handlers;
using DFC.ServiceTaxonomy.ContentApproval.Migrations;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Permissions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<ContentApprovalPart>()
                .UseDisplayDriver<ContentApprovalPartDisplayDriver>();
            
            services.AddScoped<IDataMigration, ContentApprovalPartMigrations>();
            services.AddScoped<IContentHandler, UpdateContentApprovalStatusHandler>();


            services.AddScoped<IPermissionProvider, CanPerformReviewPermissions>();
            services.AddScoped<IPermissionProvider, CanPerformApprovalPermissions>();
            services.AddScoped<IPermissionProvider, RequestReviewPermissions>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.ContentApproval",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
