using System;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.GraphSync.Drivers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.Handlers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Configuration;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.GraphSync
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.Configure<Neo4jConfiguration>(configuration.GetSection("Neo4j"));
            services.Configure<NamespacePrefixConfiguration>(configuration.GetSection("GraphSync"));

            // Graph Database
            services.AddSingleton<IGraphDatabase, NeoGraphDatabase>();
            services.AddTransient<IMergeNodeCommand, MergeNodeCommand>();
            services.AddTransient<IReplaceRelationshipsCommand, ReplaceRelationshipsCommand>();

            // Sync to graph workflow task
            services.AddActivity<SyncToGraphTask, SyncToGraphTaskDisplay>();

            // Syncers
            services.AddTransient<IGraphSyncer, GraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, TitlePartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, BagPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, EponymousPartGraphSyncer>();
            services.AddTransient<IGraphSyncPartIdProperty, GraphSyncPartUriIdProperty>();

            // Graph Sync Part
            services.AddContentPart<GraphSyncPart>();
            services.AddScoped<IContentPartDisplayDriver, GraphSyncPartDisplayDriver>();
            //services.AddScoped<IContentPartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartHandler, GraphSyncPartHandler>();
            services.AddScoped<IContentPartGraphSyncer, GraphSyncPartGraphSyncer>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.GraphSync",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
