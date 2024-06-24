using DFC.ServiceTaxonomy.CompUi.AppRegistry;
using DFC.ServiceTaxonomy.CompUi.BackgroundTask;
using DFC.ServiceTaxonomy.CompUi.BackgroundTask.Activity;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Indexes;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using DFC.ServiceTaxonomy.CompUi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
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
            services.AddSingleton<IPageLocationUpdater, PageLocationUpdater>();
            services.AddSingleton<IBackgroundTask, RefreshCacheOnPublish>();
            services.AddSingleton<IBackgroundQueue<Processing>, BackgroundQueue<Processing>>();
            services.AddSingleton<IBackgroundItemQueueMonitor, BackgroundItemQueueMonitor>();
            services.AddSingleton<IJobProfileCacheRefresh, JobProfileCacheRefresh>();

            services.AddScoped<IDirector, Director>();
            services.AddScoped<IBuilder, ConcreteBuilder>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
