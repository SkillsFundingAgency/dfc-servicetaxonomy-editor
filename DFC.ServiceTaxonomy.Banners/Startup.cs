using DFC.ServiceTaxonomy.Banners.Drivers;
using DFC.ServiceTaxonomy.Banners.Indexes;
using DFC.ServiceTaxonomy.Banners.Models;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Banners
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<BannerPart>()
                .UseDisplayDriver<BannerPartDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IIndexProvider, BannerPartIndexProvider>();
        }
    }
}

