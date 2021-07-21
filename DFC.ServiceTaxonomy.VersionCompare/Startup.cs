using System;
using DFC.ServiceTaxonomy.VersionCompare.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace DFC.ServiceTaxonomy.VersionCompare
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "VersionCompareIndex",
                areaName: "DFC.ServiceTaxonomy.VersionCompare",
                pattern: _adminOptions.AdminUrlPrefix + "/VersionCompare/{contentId}/{baseVersion?}/{compareVersion?}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index)}
                );
        }
    }
}
