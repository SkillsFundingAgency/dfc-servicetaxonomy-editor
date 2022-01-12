using System;
using DFC.ServiceTaxonomy.VersionComparison.Controllers;
using DFC.ServiceTaxonomy.VersionComparison.Drivers;
using DFC.ServiceTaxonomy.VersionComparison.Services;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Display.ContentDisplay;
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
            services.AddScoped<IDisplayManager<DiffItem>, DisplayManager<DiffItem>>()
                .AddScoped<IDisplayDriver<DiffItem>, DiffItemDisplayDriver>();

            services.AddScoped<IAuditTrailQueryService, AuditTrailQueryService>();
            services.AddScoped<IDiffBuilderService, DiffBuilderService>();
            services.AddScoped<IContentNameService, ContentNameService>();

            services.AddScoped<IContentDisplayDriver, VersionComparisonContentsDriver>();

            // Ordering of adding these services is important here, they will be processed for appropriateness in this order
            services
                .AddScoped<IPropertyService, NullPropertyService>()
                .AddScoped<IPropertyService, BasicPropertyService>()
                .AddScoped<IPropertyService, AddBannerPropertyService>()
                .AddScoped<IPropertyService, PageLocationsPropertyService>()
                .AddScoped<IPropertyService, WidgetPropertyService>()
                .AddScoped<IPropertyService, SingleChildPropertyService>();
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
