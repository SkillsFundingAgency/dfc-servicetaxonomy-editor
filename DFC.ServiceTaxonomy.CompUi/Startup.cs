using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Indexes;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.CompUi
{
    [RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Sitemaps")]
    public class Startup : StartupBase
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
            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddMemoryCache();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, RelatedContentItemIndexProvider>();
            services.AddSingleton<IDataService, DataService>();
            
            services.AddScoped<IEventGridHandler, EventGridHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
