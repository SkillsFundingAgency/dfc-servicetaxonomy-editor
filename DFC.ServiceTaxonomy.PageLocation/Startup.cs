using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.PageLocation.Drivers;
using DFC.ServiceTaxonomy.PageLocation.GraphSyncers;
using DFC.ServiceTaxonomy.PageLocation.Handlers;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.Validators;
using DFC.ServiceTaxonomy.PageLocation.ViewModels;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<PageLocationPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<PageLocationPart>()
                .UseDisplayDriver<PageLocationPartDisplayDriver>()
                .AddHandler<PageLocationPartHandler>();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, PageLocationPartIndexProvider>();

            services.AddTransient<IContentPartGraphSyncer, PageLocationPartGraphSyncer>();

            services.AddScoped<IContentHandler, DefaultPageLocationsContentHandler>();

            services.AddScoped<IContentDisplayDriver, PageLocationDriver>();

            services.AddTransient<ITaxonomyTermValidator, PageLocationUrlValidator>();
            services.AddTransient<ITaxonomyTermValidator, PageLocationTitleValidator>();

            services.AddTransient<ITaxonomyTermHandler, PageLocationTaxonomyTermHandler>();
        }
    }
}
