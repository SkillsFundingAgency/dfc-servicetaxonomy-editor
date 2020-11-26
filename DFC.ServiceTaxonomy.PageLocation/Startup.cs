using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.PageLocation.Drivers;
using DFC.ServiceTaxonomy.PageLocation.Filters;
using DFC.ServiceTaxonomy.PageLocation.GraphSyncers;
using DFC.ServiceTaxonomy.PageLocation.Handlers;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.Services;
using DFC.ServiceTaxonomy.PageLocation.Validators;
using DFC.ServiceTaxonomy.PageLocation.ViewModels;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Fluid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<PageLocationPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<PageLocationPartSettingsViewModel>();
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
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, PageLocationPartSettingsDisplayDriver>();

            services.AddTransient<ITaxonomyTermValidator, PageLocationUrlValidator>();
            services.AddTransient<ITaxonomyTermValidator, PageLocationTitleValidator>();
            services.AddTransient<ITaxonomyValidator, PageLocationsTaxonomyValidator>();
            services.AddTransient<ITaxonomyTermUpdateValidator, PageLocationUpdateOrDeleteValidator>();
            services.AddTransient<ITaxonomyTermDeleteValidator, PageLocationUpdateOrDeleteValidator>();

            services.AddTransient<ITaxonomyTermHandler, PageLocationTaxonomyTermHandler>();

            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(ResourceInjectionFilter));
            });

            services.AddTransient<IPageLocationClonePropertyGenerator, PageLocationClonePropertyGenerator>();
        }
    }
}
