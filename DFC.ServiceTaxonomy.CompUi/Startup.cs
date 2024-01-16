using DFC.Common.SharedContent.Pkg.Netcore;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.IUtilities;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using YesSql;

namespace DFC.ServiceTaxonomy.CompUi
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IContentHandler, CacheHandler>();
            services.AddTransient<IDapperWrapper, DapperWrapper>();
            services.AddTransient<IUtilities, Utilities>();
            //Todo add the code to read this from the appsettings.json
            //services.AddSharedContentRedisInterface("dfc-dev-shared-rdc.redis.cache.windows.net:6380,password=Nuzqmeax2bVwFYQQ7YCbDcxexbtBNUuyyAzCaOtGPLo=,ssl=True,abortConnect=False");

            services.AddTransient<ISharedContentRedisInterface, SharedContentRedis>();

            services.AddAutoMapper(typeof(Startup).Assembly);
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
