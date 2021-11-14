using DFC.ServiceTaxonomy.Title.Drivers;
using DFC.ServiceTaxonomy.Title.GraphSyncers;
using DFC.ServiceTaxonomy.Title.Indexes;
using DFC.ServiceTaxonomy.Title.Models;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Title
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<UniqueTitlePart>()
                .UseDisplayDriver<UniqueTitlePartDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IIndexProvider, UniqueTitlePartIndexProvider>();

            services.AddTransient<IContentPartGraphSyncer, UniqueTitlePartGraphSyncer>();

        }
    }
}

