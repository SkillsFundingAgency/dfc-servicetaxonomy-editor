using System;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.Editor.Module
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<UriField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayUriFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<Neo4jConfiguration>(configuration.GetSection("Neo4j"));
            services.AddSingleton<INeoGraphDatabase, NeoGraphDatabase>();
            services.AddActivity<SyncToGraphTask, SyncToGraphTaskDisplay>();

            // Uri Field
            services.AddContentField<UriField>();
            services.AddScoped<IContentFieldDisplayDriver, UriFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, UriFieldSettingsDriver>();
            // services.AddScoped<IContentFieldIndexHandler, UriFieldIndexHandler>();
            // services.AddScoped<IContentPartFieldDefinitionDisplayDriver, UriFieldPredefinedListEditorSettingsDriver>();
            // services.AddScoped<IContentPartFieldDefinitionDisplayDriver, UriFieldHeaderDisplaySettingsDriver>();

        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.Editor.Module",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
