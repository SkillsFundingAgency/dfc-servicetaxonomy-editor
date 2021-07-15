using System;
using DFC.ServiceTaxonomy.Media.Events;
using DFC.ServiceTaxonomy.Media.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.Events;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.Media
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IKeyVaultService, KeyVaultService>();
            services.AddSingleton<ICdnService, CdnService>();
            services.AddSingleton<IMediaEventHandler, MediaBlobStoreEventHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.Media",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "index" }
            );
        }
    }
}
