using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents.Controllers;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Security.Permissions;
using Rework.ContentApproval.Controllers;
using Rework.ContentApproval.Drivers;
using Rework.ContentApproval.Handlers;
using Rework.ContentApproval.Indexes;
using Rework.ContentApproval.Models;
using Rework.ContentApproval.Shapes;
using YesSql.Indexes;
using Microsoft.Extensions.Options;

namespace Rework.ContentApproval
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddContentPart<ContentApprovalPart>()
                .UseDisplayDriver<ContentApprovalPartDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, ContentApprovalPartIndexProvider>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IShapeTableProvider, ContentShapes>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
            var approvalControllerName = typeof(ApprovalController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "EditForcePublishContentItem",
                areaName: "Rework.ContentApproval",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/EditForcePublish",
                //do we need a placeholder edit (get) action??
                //                defaults: new { controller = approvalControllerName, action = nameof(ApprovalController.Edit) }
                defaults: new
                {
                    //                    controller = approvalControllerName, action = nameof(ApprovalController.EditAndPublishPOST)
                    controller = approvalControllerName,
                    action = nameof(ApprovalController.Edit)
                }
            );
        }
    }

    [RequireFeatures("OrchardCore.Workflows")]
    public class StartupWithWorkflows : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<ContentApprovalPart>()
                .AddHandler<ContentApprovalPartHandler>();
        }
    }
}
