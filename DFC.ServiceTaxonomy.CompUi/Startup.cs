using DFC.ServiceTaxonomy.CompUi.AppRegistry;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.CompUi
{
    [RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Sitemaps")]
    public class Startup : OrchardCore.Modules.StartupBase
    {
        private IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IContentHandler, CacheHandler>();
            services.AddTransient<IDapperWrapper, DapperWrapper>();
            services.AddSingleton<IPageLocationUpdater, PageLocationUpdater>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            
        }
    }
}
