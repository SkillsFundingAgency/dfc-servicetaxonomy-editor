using System;
using DFC.ServiceTaxonomy.ContentPickerPreview.Controllers;
using DFC.ServiceTaxonomy.ContentPickerPreview.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace DFC.ServiceTaxonomy.ContentPickerPreview
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPickerResultProvider, PreviewContentPickerResultProvider>();
            services.AddScoped<IBannerContentPickerResultProvider, BannerContentPickerResultProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseStaticFiles();

            routes.MapAreaControllerRoute(
                name: "BannerContentPicker",
                areaName: "DFC.ServiceTaxonomy.ContentPickerPreview",
                pattern: "Admin/BannerContentPicker/SearchContentItems",
                defaults: new { controller = typeof(BannerContentPickerAdminController).ControllerName(), action = nameof(BannerContentPickerAdminController.SearchContentItems) }
              );
        }
    }
}
