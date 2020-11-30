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
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrchardCore.Workflows.Helpers;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Flow;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using OrchardCore.DisplayManagement.Notify;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using DFC.ServiceTaxonomy.Slack;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;

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

            services.Configure<GraphSyncSettings>(_configuration.GetSection(nameof(GraphSyncSettings)));

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
            services.AddScoped<IContentDefinitionEventHandler, GraphSyncContentDefinitionHandler>();
            services.AddTransient<IGetIncomingContentPickerRelationshipsQuery, GetIncomingContentPickerRelationshipsQuery>();

            // GraphSyncPart
            services.AddContentPart<GraphSyncPart>()
                .UseDisplayDriver<GraphSyncPartDisplayDriver>()
                .AddHandler<GraphSyncPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddTransient<IContentPartGraphSyncer, GraphSyncPartGraphSyncer>();
            services.AddTransient<IGraphSyncPartGraphSyncer, GraphSyncPartGraphSyncer>();
            services.AddSingleton<IIndexProvider, GraphSyncPartIndexProvider>();

            // orchestrators & orchestration handlers
            services.AddTransient<IDeleteOrchestrator, DeleteOrchestrator>();
            services.AddTransient<ISyncOrchestrator, SyncOrchestrator>();
            services.AddTransient<IContentTypeOrchestrator, ContentTypeOrchestrator>();
            services.AddTransient<IContentOrchestrationHandler, EventGridPublishingHandler>();

            // syncers
            services.AddTransient<IMergeGraphSyncer, MergeGraphSyncer>();
            services.AddTransient<IDeleteGraphSyncer, DeleteGraphSyncer>();
            services.AddTransient<IDeleteTypeGraphSyncer, DeleteTypeGraphSyncer>();
            services.AddTransient<ICloneGraphSync, CloneGraphSync>();
            services.AddTransient<IValidateAndRepairGraph, ValidateAndRepairGraph>();
            services.AddTransient<IGraphResyncer, GraphResyncer>();
            services.AddTransient<IVisualiseGraphSyncer, VisualiseGraphSyncer>();

            services.AddTransient<ISyncNameProvider, SyncNameProvider>();
            services.AddTransient<ISyncNameProviderCSharpScriptGlobals, SyncNameProviderCSharpScriptGlobals>();
            services.AddTransient<IGraphValidationHelper, GraphValidationHelper>();
            services.AddTransient<IContentFieldsGraphSyncer, ContentFieldsGraphSyncer>();
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
            services.AddTransient<ITermPartGraphSyncer, TermPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, EponymousPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, HtmlBodyPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, PublishLaterPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, UnpublishLaterPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, SitemapPartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, AutoroutePartGraphSyncer>();
            services.AddTransient<IContentPartGraphSyncer, AliasPartGraphSyncer>();

            // field syncers
            services.AddTransient<IContentFieldGraphSyncer, TextFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, BooleanFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, NumericFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, HtmlFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, LinkFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, ContentPickerFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, DateTimeFieldGraphSyncer>();
            services.AddTransient<IContentFieldGraphSyncer, TaxonomyFieldGraphSyncer>();

            // workflow activities
            services.AddActivity<AuditSyncIssuesTask, AuditSyncIssuesTaskDisplay>();

            // notifiers
            services.Replace(ServiceDescriptor.Scoped<IGraphSyncNotifier, GraphSyncNotifier>());
            services.Replace(ServiceDescriptor.Scoped<INotifier>(sp => sp.GetRequiredService<IGraphSyncNotifier>()));

            // services
            services.AddScoped<ISynonymService, SynonymService>();
            services.AddTransient<ITitlePartCloneGenerator, TitlePartCloneGenerator>();
            services.AddTransient<IContentItemVersionFactory, ContentItemVersionFactory>();
            services.AddTransient<INodeContentItemLookup, NodeContentItemLookup>();
            services.AddTransient<IDescribeContentItemHelper, DescribeContentItemHelper>();
            // this would be nice, but IContentManager is Scoped, so not available at startup
            //services.AddSingleton<IPublishedContentItemVersion>(sp => new PublishedContentItemVersion(_configuration, sp.GetRequiredService<IContentManager>()));
            services.AddSingleton<IPublishedContentItemVersion>(new PublishedContentItemVersion(_configuration));
            services.AddSingleton<IPreviewContentItemVersion>(new PreviewContentItemVersion(_configuration));
            services.AddSingleton<INeutralEventContentItemVersion>(new NeutralEventContentItemVersion());
            services.AddSingleton<ISuperpositionContentItemVersion>(new SuperpositionContentItemVersion());
            services.AddSingleton<IEscoContentItemVersion>(new EscoContentItemVersion());

            // permissions
            services.AddScoped<IPermissionProvider, Permissions>();

            // navigation
            services.AddScoped<INavigationProvider, AdminMenu>();

            //slack message publishing
            services.AddSlackMessagePublishing(_configuration, true);
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
