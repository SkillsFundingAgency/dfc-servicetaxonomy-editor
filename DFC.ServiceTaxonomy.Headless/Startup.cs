using DFC.ServiceTaxonomy.Headless.GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Media.Fields;
using OrchardCore.Modules;
using OrchardCore.Sitemaps.Models;

namespace DFC.ServiceTaxonomy.Headless
{
    [RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Sitemaps", "OrchardCore.Media")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<SitemapPart, SitemapPartQueryObjectType>();
            services.AddObjectGraphType<MediaField, GraphQL.MediaFieldQueryObjectType>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

        }
    }
}
