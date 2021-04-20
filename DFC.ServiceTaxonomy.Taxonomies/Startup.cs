using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using DFC.ServiceTaxonomy.Taxonomies.Controllers;
using DFC.ServiceTaxonomy.Taxonomies.Drivers;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using DFC.ServiceTaxonomy.Taxonomies.GraphQL;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using DFC.ServiceTaxonomy.Taxonomies.Indexing;
using DFC.ServiceTaxonomy.Taxonomies.Liquid;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.Services;
using DFC.ServiceTaxonomy.Taxonomies.Settings;
using DFC.ServiceTaxonomy.Taxonomies.ViewModels;
using DFC.ServiceTaxonomy.Taxonomies.Helper;

namespace DFC.ServiceTaxonomy.Taxonomies
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            // Registering both field types and shape types are necessary as they can
            // be accessed from inner properties.

            TemplateContext.GlobalMemberAccessStrategy.Register<TaxonomyField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTaxonomyFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTaxonomyFieldTagsViewModel>();
        }

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IShapeTableProvider, TermShapes>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // Taxonomy Part
            services.AddContentPart<TaxonomyPart>()
                .UseDisplayDriver<TaxonomyPartDisplayDriver>()
                .AddHandler<TaxonomyPartHandler>();

            // Taxonomy Field
            services.AddContentField<TaxonomyField>()
                .UseDisplayDriver<TaxonomyFieldDisplayDriver>(d => !String.Equals(d, "Tags", StringComparison.OrdinalIgnoreCase));

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TaxonomyFieldIndexHandler>();

            // Taxonomy Tags Display Mode and Editor.
            services.AddContentField<TaxonomyField>()
                .UseDisplayDriver<TaxonomyFieldTagsDisplayDriver>(d => String.Equals(d, "Tags", StringComparison.OrdinalIgnoreCase));

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldTagsEditorSettingsDriver>();

            services.AddScoped<IScopedIndexProvider, TaxonomyIndexProvider>();

            // Terms.
            services.AddContentPart<TermPart>();
            services.AddScoped<IContentHandler, TermPartContentHandler>();
            services.AddScoped<IContentDisplayDriver, TermPartContentDriver>();

            // Liquid
            services.AddLiquidFilter<TaxonomyTermsFilter>("taxonomy_terms");
            services.AddLiquidFilter<InheritedTermsFilter>("inherited_terms");

            // Helper.
            services.AddSingleton<ITaxonomyHelper, TaxonomyHelper>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var taxonomyControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Taxonomies.Create",
                areaName: "DFC.ServiceTaxonomy.Taxonomies",
                pattern: _adminOptions.AdminUrlPrefix + "/Taxonomies/Create/{id}",
                defaults: new { controller = taxonomyControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Taxonomies.Edit",
                areaName: "DFC.ServiceTaxonomy.Taxonomies",
                pattern: _adminOptions.AdminUrlPrefix + "/Taxonomies/Edit/{taxonomyContentItemId}/{taxonomyItemId}",
                defaults: new { controller = taxonomyControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "Taxonomies.Delete",
                areaName: "DFC.ServiceTaxonomy.Taxonomies",
                pattern: _adminOptions.AdminUrlPrefix + "/Taxonomies/Delete/{taxonomyContentItemId}/{taxonomyItemId}",
                defaults: new { controller = taxonomyControllerName, action = nameof(AdminController.Delete) }
            );
        }
    }

    [Feature("DFC.ServiceTaxonomy.Taxonomies.ContentsAdminList")]
    public class ContentsAdminListStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentsAdminListFilter, TaxonomyContentsAdminListFilter>();
            services.AddScoped<IDisplayDriver<ContentOptionsViewModel>, TaxonomyContentsAdminListDisplayDriver>();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, TaxonomyContentsAdminListSettingsDisplayDriver>();
        }
    }

    [Feature("DFC.ServiceTaxonomy.Taxonomies.ContentsAdminList")]
    [RequireFeatures("OrchardCore.Deployment")]
    public class ContentsAdminListDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<TaxonomyContentsAdminListSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<ContentsAdminListDeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<TaxonomyContentsAdminListSettings>(S["Taxonomy Filters settings"], S["Exports the Taxonomy filters settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<TaxonomyContentsAdminListSettings>());
        }
    }

    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class GraphQLStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<TaxonomyPart, TaxonomyPartQueryObjectType>();
            services.AddObjectGraphType<TaxonomyField, TaxonomyFieldQueryObjectType>();
        }
    }
}
