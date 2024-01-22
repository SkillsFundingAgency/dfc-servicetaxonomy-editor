using DFC.Common.SharedContent.Pkg.Netcore;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.IUtilities;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using Microsoft.Extensions.Configuration;
using DFC.Common.SharedContent.Pkg.Netcore.Infrastructure;

namespace DFC.ServiceTaxonomy.CompUi
{
    public class Startup : OrchardCore.Modules.StartupBase
    {
        private const string RedisCacheConnectionStringAppSettings = "Cms:RedisCacheConnectionString";

        private IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IContentHandler, CacheHandler>();
            services.AddTransient<IDapperWrapper, DapperWrapper>();
            services.AddTransient<IUtilities, Utilities>();

            services.AddStackExchangeRedisCache(options => { options.Configuration = configuration.GetSection(RedisCacheConnectionStringAppSettings).Get<string>(); });
            services.AddSingleton<ISharedContentRedisInterfaceStrategyFactory, SharedContentRedisStrategyFactory>();
            services.AddScoped<ISharedContentRedisInterface, SharedContentRedis>();

            services.AddAutoMapper(typeof(Startup).Assembly);
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
