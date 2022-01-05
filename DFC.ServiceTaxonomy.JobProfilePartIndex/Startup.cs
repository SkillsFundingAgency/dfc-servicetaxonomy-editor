//using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;
using OrchardCore.ContentTypes.Editors;
using DFC.ServiceTaxonomy.JobProfilePartIndex.Indexes;

namespace DFC.ServiceTaxonomy.JobProfilePartIndex
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddContentPart<UniqueTitlePart>()
            //    .UseDisplayDriver<UniqueTitlePartDisplayDriver>();
            //services.AddScoped<IContentTypePartDefinitionDisplayDriver, UniqueTitlePartSettingsDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IIndexProvider, JobProfileIndexProvider>();

            //services.AddTransient<IContentPartGraphSyncer, UniqueTitlePartGraphSyncer>();

        }
    }
}

