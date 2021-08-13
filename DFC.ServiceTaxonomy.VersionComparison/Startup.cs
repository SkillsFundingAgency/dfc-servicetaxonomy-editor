using System;
using DFC.ServiceTaxonomy.VersionComparison.Controllers;
using DFC.ServiceTaxonomy.VersionComparison.Drivers;
using DFC.ServiceTaxonomy.VersionComparison.Services;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace DFC.ServiceTaxonomy.VersionComparison
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
            services.AddScoped<IDisplayManager<VersionComparisonOptions>, DisplayManager<VersionComparisonOptions>>()
                .AddScoped<IDisplayDriver<VersionComparisonOptions>, VersionComparisonOptionsDisplayDriver>();

            services.AddScoped<IAuditTrailQueryService, AuditTrailQueryService>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "VersionCompareIndex",
                areaName: "DFC.ServiceTaxonomy.VersionComparison",
                pattern: _adminOptions.AdminUrlPrefix + "/VersionComparison/{contentItemId}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );
        }
    }
}
