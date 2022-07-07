using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Drivers;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.GraphSyncers.Parts.Bag;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.GraphSyncers.Parts.Flow;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.GraphSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.GraphSync.Drivers;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Flow;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Form;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.GraphSync.Handlers;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Slack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Helpers;
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

        private static async Task<CosmosDbService> InitialiseCosmosClientInstanceAsync(IConfigurationSection configurationSection)
        {
            var cosmosDbOptions = new CosmosDbOptions();
            configurationSection.Bind(cosmosDbOptions);

            if (!cosmosDbOptions.Endpoints.Any())
            {
                throw new GraphClusterConfigurationErrorException("No endpoints configured.");
            }

            const string PartitionKeyKey = "ContentType";
            var concurrentDictionary = new ConcurrentDictionary<string, Container>();
            CosmosClient? sharedClient = null;
            var firstConnString = cosmosDbOptions.Endpoints!.Values.First().ConnectionString;

            if (cosmosDbOptions.Endpoints.Values.All(v => v.ConnectionString == firstConnString))
            {
                sharedClient = new CosmosClient(firstConnString);
            }

            foreach (var endpoint in cosmosDbOptions.Endpoints!)
            {
                var client = sharedClient ?? new CosmosClient(endpoint.Value.ConnectionString);
                var db = (await client.CreateDatabaseIfNotExistsAsync(endpoint.Value.DatabaseName, ThroughputProperties.CreateManualThroughput(400))).Database;
                var containerName = endpoint.Value.ContainerName ?? endpoint.Key;
                var container = await db.CreateContainerIfNotExistsAsync(containerName, $"/{PartitionKeyKey}");

                if (!concurrentDictionary.TryAdd(endpoint.Key, container.Container))
                {
                    throw new GraphClusterConfigurationErrorException($"Failed to configure container with key '{endpoint.Key}'");
                }
            }

            return new CosmosDbService(concurrentDictionary);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<GraphSyncSettings>(_configuration.GetSection(nameof(GraphSyncSettings)));

            // recipe steps
            services.AddRecipeExecutionStep<CypherCommandStep>();
            services.AddRecipeExecutionStep<ContentNoCacheStep>();
            services.AddRecipeExecutionStep<CSharpContentStep>();
            services.AddTransient<ICypherToContentCSharpScriptGlobals, CypherToContentCSharpScriptGlobals>();
            services.AddTransient<IContentHelper, ContentHelper>();
            services.AddTransient<IGetContentItemsAsJsonQuery, GetContentItemsAsJsonQuery>();

            services.AddSingleton<ICosmosDbService>(InitialiseCosmosClientInstanceAsync(_configuration.GetSection(CosmosDbOptions.CosmosDb)).GetAwaiter().GetResult());

            services.AddGraphCluster();

            services.Configure<GraphSyncPartSettingsConfiguration>(_configuration.GetSection(nameof(GraphSyncPartSettings)));

            // graph database
            //todo: should we do this in each library that used the graph cluster, or just once in the root editor project?
            //services.AddGraphCluster();
            services.AddSingleton(sp => (IGraphClusterLowLevel)sp.GetRequiredService<IGraphCluster>());
            services.AddScoped<IContentHandler, GraphSyncContentHandler>();
            services.AddScoped<IContentDefinitionEventHandler, GraphSyncContentDefinitionHandler>();
            services.AddTransient<IGetIncomingContentPickerRelationshipsQuery, CosmosDbGetIncomingContentPickerRelationshipsQuery>();

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
            services.AddTransient<IValidateAndRepairGraph, CosmosDbValidateAndRepairGraph>();
            services.AddTransient<IGraphResyncer, GraphResyncer>();
            services.AddTransient<IVisualiseGraphSyncer, VisualiseGraphSyncer>();

            services.AddTransient<ISyncNameProvider, SyncNameProvider>();
            services.AddTransient<IGraphValidationHelper, GraphValidationHelper>();
            services.AddTransient<IContentFieldsGraphSyncer, ContentFieldsGraphSyncer>();
            services.AddTransient<IBagPartEmbeddedContentItemsGraphSyncer, CosmosDbBagPartEmbeddedContentItemsGraphSyncer>();
            services.AddTransient<IFlowPartEmbeddedContentItemsGraphSyncer, CosmosDbFlowPartEmbeddedContentItemsGraphSyncer>();
            services.AddTransient<ITaxonomyPartEmbeddedContentItemsGraphSyncer, CosmosDbTaxonomyPartEmbeddedContentItemsGraphSyncer>();

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
            services.AddTransient<IContentPartGraphSyncer, FormPartGraphSyncer>();

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
            services.AddScoped<ISynonymService, CosmosDbSynonymService>();
            services.AddTransient<ITitlePartCloneGenerator, TitlePartCloneGenerator>();
            services.AddTransient<IContentItemVersionFactory, ContentItemVersionFactory>();
            services.AddTransient<INodeContentItemLookup, NodeContentItemLookup>();
            services.AddTransient<IDescribeContentItemHelper, CosmosDbDescribeContentItemHelper>();
            services.AddSingleton<IPublishedContentItemVersion>(new PublishedContentItemVersion(_configuration));
            services.AddSingleton<IPreviewContentItemVersion>(new PreviewContentItemVersion(_configuration));
            services.AddSingleton<INeutralEventContentItemVersion>(new NeutralEventContentItemVersion());
            services.AddSingleton<ISuperpositionContentItemVersion>(new SuperpositionContentItemVersion());
            services.AddSingleton<IEscoContentItemVersion>(new EscoContentItemVersion());

            // permissions
            services.AddScoped<IPermissionProvider, Permissions>();

            // This is being removed but left in code if needed in the future
            // Whilst needed for Neo4j, an alternative approach of validating and correcting for Cosmos DB is safer 
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
