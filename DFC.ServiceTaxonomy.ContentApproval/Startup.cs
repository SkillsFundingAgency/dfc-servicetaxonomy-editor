using System;
using DFC.ServiceTaxonomy.ContentApproval.Drivers;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Migrations;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Permissions;
using DFC.ServiceTaxonomy.ContentApproval.Shapes;
using DFC.ServiceTaxonomy.ContentApproval.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<ContentApprovalPart>()
                .UseDisplayDriver<ContentApprovalPartDisplayDriver>();
            services.AddScoped<IDataMigration, ContentApprovalPartMigrations>();
            services.AddSingleton<IIndexProvider, ContentApprovalPartIndexProvider>();
            services.AddScoped<IPermissionProvider, CanPerformReviewPermissions>();
            services.AddScoped<IPermissionProvider, CanPerformApprovalPermissions>();
            services.AddScoped<IPermissionProvider, RequestReviewPermissions>();

            services.AddScoped<IContentItemsApprovalService, ContentItemsApprovalService>();

            services.AddContentPart<ContentApprovalItemStatusDashboardPart>()
                .UseDisplayDriver<ContentApprovalItemStatusDashboardPartDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IShapeTableProvider, UserEditShapes>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "ContentApproval",
                areaName: "DFC.ServiceTaxonomy.ContentApproval",
                pattern: "ContentApproval/RequestApproval",
                defaults: new { controller = "ContentApproval", action = "RequestApproval" }
            );
        }
    }
}
