using DFC.ServiceTaxonomy.Banners.Drivers;
using DFC.ServiceTaxonomy.Banners.GraphQL;
using DFC.ServiceTaxonomy.Banners.GraphSyncers;
using DFC.ServiceTaxonomy.Banners.Indexes;
using DFC.ServiceTaxonomy.Banners.Models;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Banners
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<BannerPart>()
                .UseDisplayDriver<BannerPartDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IIndexProvider, BannerPartIndexProvider>();

            services.AddTransient<IContentPartGraphSyncer, BannerPartGraphSyncer>();

            services.AddObjectGraphType<BannerPart, BannerPartQueryObjectType>();

            services.AddInputObjectGraphType<BannerPart, BannerPartInputObjectType>();

            services.AddTransient<IIndexAliasProvider, BannerPartIndexAliasProvider>();

        }
    }
}

