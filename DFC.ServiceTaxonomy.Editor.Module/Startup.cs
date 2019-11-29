using System;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1128:Unused \"using\" should be removed", Justification = "only temporary")]

namespace DFC.ServiceTaxonomy.Editor.Module
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            //todo: what's this doing?
            // TemplateContext.GlobalMemberAccessStrategy.Register<UriField>();
            //once we have a custom 1
            //TemplateContext.GlobalMemberAccessStrategy.Register<DisplayUriFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<Neo4jConfiguration>(configuration.GetSection("Neo4j"));
            services.AddSingleton<INeoGraphDatabase, NeoGraphDatabase>();
            // no mention of this being necessary in the docs
            services.AddActivity<SyncToGraphTask, SyncToGraphTaskDisplay>();

            // Uri Field
            // services.AddContentField<UriField>();
            // services.AddScoped<IContentFieldDisplayDriver, UriFieldDisplayDriver>();
            // // services.AddScoped<IContentPartFieldDefinitionDisplayDriver, UriFieldSettingsDriver>();
            // // services.AddScoped<IContentFieldIndexHandler, UriFieldIndexHandler>();

            // what the docs say :-)
            // services.AddSingleton<ContentField, UriField>();
            // services.AddScoped<IContentFieldDisplayDriver, UriFieldDisplayDriver>();
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
