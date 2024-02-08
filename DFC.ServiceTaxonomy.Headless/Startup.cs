using DFC.ServiceTaxonomy.Headless.GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Modules;
using OrchardCore.Sitemaps.Models;

namespace DFC.ServiceTaxonomy.Headless
{
    [RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Sitemaps")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<SitemapPart, SitemapPartQueryObjectType>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

        }
    }
}
