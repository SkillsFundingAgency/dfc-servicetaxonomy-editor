﻿using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.PageLocation.Drivers;
using DFC.ServiceTaxonomy.PageLocation.Filters;
using DFC.ServiceTaxonomy.PageLocation.GraphQL;
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
using Microsoft.Extensions.Options;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
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
            services.AddTransient<ITaxonomyTermValidator, PageLocationModificationValidator>();
            services.AddTransient<ITaxonomyValidator, PageLocationsTaxonomyValidator>();

            services.AddTransient<ITaxonomyTermHandler, PageLocationTaxonomyTermHandler>();

            services.AddInputObjectGraphType<PageLocationPart, PageLocationInputObjectType>();
            services.AddTransient<IIndexAliasProvider, PageLocationPartIndexAliasProvider>();
            services.AddObjectGraphType<PageLocationPart, PageLocationPartQueryObjectType>();

            services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(ResourceInjectionFilter));
            });

            services.AddTransient<IPageLocationClonePropertyGenerator, PageLocationClonePropertyGenerator>();

            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<PageLocationPartViewModel>();
                o.MemberAccessStrategy.Register<PageLocationPartSettingsViewModel>();
            });
        }
    }
}
