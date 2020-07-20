using System;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using DFC.ServiceTaxonomy.GraphSync.Drivers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.Handlers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrchardCore.Workflows.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Drivers.Events;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Flow;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.ContentTypes.Services;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Managers.Interface;
using DFC.ServiceTaxonomy.GraphSync.Managers;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.GraphSync
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // recipe steps
            services.AddRecipeExecutionStep<CypherCommandStep>();
            services.AddRecipeExecutionStep<CypherToContentStep>();
            services.AddRecipeExecutionStep<CSharpContentStep>();
            services.AddRecipeExecutionStep<ContentNoCacheStep>();
            services.AddTransient<ICypherToContentCSharpScriptGlobals, CypherToContentCSharpScriptGlobals>();
            services.AddTransient<IContentHelper, ContentHelper>();
            services.AddTransient<IServiceTaxonomyHelper, ServiceTaxonomyHelper>();
            services.AddTransient<IGetContentItemsAsJsonQuery, GetContentItemsAsJsonQuery>();

            // graph database
            //todo: should we do this in each library that used the graph cluster, or just once in the root editor project?
            //services.AddGraphCluster();
            services.AddSingleton(sp => (IGraphClusterLowLevel)sp.GetRequiredService<IGraphCluster>());
            services.AddScoped<IContentHandler, GraphSyncContentHandler>();

            // GraphSyncPart
            services.AddContentPart<GraphSyncPart>()
                .UseDisplayDriver<GraphSyncPartDisplayDriver>()
                .AddHandler<GraphSyncPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartGraphSyncer, GraphSyncPartGraphSyncer>();

            // syncers
            services.AddTransient<IMergeGraphSyncer, MergeGraphSyncer>();
            services.AddTransient<IDeleteGraphSyncer, DeleteGraphSyncer>();
            services.AddTransient<IValidateAndRepairGraph, ValidateAndRepairGraph>();

            services.AddTransient<IGraphSyncHelper, GraphSyncHelper>();
            services.AddTransient<IGraphSyncHelperCSharpScriptGlobals, GraphSyncHelperCSharpScriptGlobals>();
            services.AddTransient<IGraphValidationHelper, GraphValidationHelper>();
            services.AddTransient<IContentFieldsGraphSyncer, ContentFieldsGraphSyncer>();
            services.AddTransient<IEmbeddedContentItemsGraphSyncer, EmbeddedContentItemsGraphSyncer>();
            services.AddTransient<IBagPartEmbeddedContentItemsGraphSyncer, BagPartEmbeddedContentItemsGraphSyncer>();
            services.AddTransient<IFlowPartEmbeddedContentItemsGraphSyncer, FlowPartEmbeddedContentItemsGraphSyncer>();
            services.AddTransient<ITaxonomyPartEmbeddedContentItemsGraphSyncer, TaxonomyPartEmbeddedContentItemsGraphSyncer>();

            // content item syncers
            services.AddTransient<IContentItemGraphSyncer, TaxonomyTermContentItemGraphSyncer>();
            services.AddTransient<IContentItemGraphSyncer, ContentItemGraphSyncer>();

            // part syncers
            services.AddTransient<IContentPartGraphSyncer, TitlePartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, BagPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, FlowPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, TaxonomyPartGraphSyncer>();
            services.AddTransient<ITaxonomyPartGraphSyncer, TaxonomyPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, EponymousPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, HtmlBodyPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, PublishLaterPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, UnpublishLaterPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, SitemapPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, AutoroutePartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, AliasPartGraphSyncer>();

            // field syncers
            services.AddTransient<IContentFieldGraphSyncer, TextFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, NumericFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, HtmlFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, LinkFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, ContentPickerFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, DateTimeFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, TaxonomyFieldGraphSyncer>();

            // workflow activities
            services.AddActivity<SyncToGraphTask, SyncToGraphTaskDisplay>();
            services.AddActivity<DeleteFromGraphTask, DeleteFromGraphTaskDisplay>();
            services.AddActivity<DeleteContentTypeFromGraphTask, DeleteContentTypeFromGraphTaskDisplay>();
            services.AddActivity<ContentTypeDeletedEvent, ContentTypeDeletedEventDisplay>();
            services.AddActivity<DeleteContentTypeTask, DeleteContentTypeTaskDisplay>();
            services.AddActivity<AuditSyncIssuesTask, AuditSyncIssuesTaskDisplay>();
            services.AddActivity<ContentTypeFieldRemovedEvent, ContentTypeFieldRemovedEventDisplay>();
            services.AddActivity<RemoveFieldFromContentItemsTask, RemoveFieldFromContentItemsTaskDisplay>();
            services.AddActivity<PublishContentTypeContentItemsTask, PublishContentTypeContentItemsTaskDisplay>();

            // notifiers
            services.Replace(ServiceDescriptor.Scoped<INotifier, CustomNotifier>());

            // services
            services.AddScoped<IOrchardCoreContentDefinitionService, OrchardCoreContentDefinitionService>();
            services.Replace(ServiceDescriptor.Scoped<IContentDefinitionService, CustomContentDefinitionService>());
            services.AddScoped<ISynonymService, SynonymService>();
            services.AddTransient<IContentItemVersionFactory, ContentItemVersionFactory>();
            // this would be nice, but IContentManager is Scoped, so not available at startup
            //services.AddSingleton<IPublishedContentItemVersion>(sp => new PublishedContentItemVersion(_configuration, sp.GetRequiredService<IContentManager>()));
            services.AddSingleton<IPublishedContentItemVersion>(new PublishedContentItemVersion(_configuration));
            services.AddSingleton<IPreviewContentItemVersion>(new PreviewContentItemVersion(_configuration));

            // managers
            services.AddScoped<ICustomContentDefintionManager, CustomContentDefinitionManager>();

            // permissions
            services.AddScoped<IPermissionProvider, Permissions>();

            // navigation
            services.AddScoped<INavigationProvider, AdminMenu>();
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
