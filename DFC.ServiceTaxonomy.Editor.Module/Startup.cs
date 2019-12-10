using System;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.Handlers;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.Editor.Module
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            //todo: are these only necessary for fluid?
            TemplateContext.GlobalMemberAccessStrategy.Register<GraphUriIdField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayGraphUriIdFieldViewModel>();

            TemplateContext.GlobalMemberAccessStrategy.Register<GraphLookupField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayGraphLookupFieldViewModel>();

            TemplateContext.GlobalMemberAccessStrategy.Register<GraphLookupPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<Neo4jConfiguration>(configuration.GetSection("Neo4j"));
            services.AddSingleton<INeoGraphDatabase, NeoGraphDatabase>();
            services.AddActivity<SyncToGraphTask, SyncToGraphTaskDisplay>();

            services.Configure<NamespacePrefixConfiguration>(configuration.GetSection("GraphUriIdField"));

            // Graph Uri Id Field
            services.AddContentField<GraphUriIdField>();
            services.AddScoped<IContentFieldDisplayDriver, GraphUriIdFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GraphUriIdFieldSettingsDriver>();
            //todo: index?
            // services.AddScoped<IContentFieldIndexHandler, GraphUriIdFieldIndexHandler>();
            // services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GraphUriIdFieldPredefinedListEditorSettingsDriver>();
            // services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GraphUriIdFieldHeaderDisplaySettingsDriver>();

            // Graph Lookup Field
            services.AddContentField<GraphLookupField>();
            services.AddScoped<IContentFieldDisplayDriver, GraphLookupFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GraphLookupFieldSettingsDriver>();

            // Graph Lookup Part
            services.AddContentPart<GraphLookupPart>();
            services.AddScoped<IContentPartHandler, GraphLookupPartHandler>();
            services.AddScoped<IContentPartDisplayDriver, GraphLookupPartDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphLookupPartSettingsDisplayDriver>();

            //todo: split content part out into own module
            services.AddScoped<IDataMigration, Migrations>();
        }

        //public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        //{
        //    routes.MapAreaControllerRoute(
        //        name: "Home",
        //        areaName: "DFC.ServiceTaxonomy.Editor.Module",
        //        pattern: "Home/Index",
        //        defaults: new { controller = "Home", action = "Index" }
        //    );
        //}

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "GraphLookup",
                areaName: "DFC.ServiceTaxonomy.Editor.Module",
                pattern: "GraphLookup/SearchLookupNodes",
                defaults: new { controller = "GraphLookupAdmin", action = "SearchLookupNodes" }
            );
        }
    }
}
