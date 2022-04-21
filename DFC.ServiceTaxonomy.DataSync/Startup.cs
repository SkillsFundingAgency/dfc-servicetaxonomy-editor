using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Activities;
using DFC.ServiceTaxonomy.DataSync.CosmosDb;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Parts.Bag;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Parts.Flow;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.DataSync.CSharpScripting;
using DFC.ServiceTaxonomy.DataSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Bag;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Flow;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Form;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.DataSync.Drivers;
using DFC.ServiceTaxonomy.DataSync.Exceptions;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Handlers;
using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Indexes;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.DataSync.Models;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using DFC.ServiceTaxonomy.DataSync.Orchestrators;
using DFC.ServiceTaxonomy.DataSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Recipes.Executors;
using DFC.ServiceTaxonomy.DataSync.Services;
using DFC.ServiceTaxonomy.DataSync.Services.Interface;
using DFC.ServiceTaxonomy.DataSync.Settings;
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

namespace DFC.ServiceTaxonomy.DataSync
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
             if(!cosmosDbOptions.Endpoints.Any())
            {
                throw new DataSyncClusterConfigurationErrorException("No endpoints configured.");
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
                concurrentDictionary.TryAdd(containerName, container.Container);
            }
            return new CosmosDbService(concurrentDictionary);
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

            services.AddSingleton<ICosmosDbService>(InitialiseCosmosClientInstanceAsync(_configuration.GetSection(CosmosDbOptions.CosmosDb)).GetAwaiter().GetResult());

            services.AddDataSyncCluster();

            services.Configure<GraphSyncPartSettingsConfiguration>(_configuration.GetSection(nameof(GraphSyncPartSettings)));

            // graph database
            //todo: should we do this in each library that used the graph cluster, or just once in the root editor project?
            //services.AddDataSyncCluster();
            services.AddSingleton(sp => (IDataSyncClusterLowLevel)sp.GetRequiredService<IDataSyncCluster>());
            services.AddScoped<IContentHandler, DataSyncContentHandler>();
            services.AddScoped<IContentDefinitionEventHandler, DataSyncContentDefinitionHandler>();
            services.AddTransient<IGetIncomingContentPickerRelationshipsQuery, CosmosDbGetIncomingContentPickerRelationshipsQuery>();

            // GraphSyncPart
            services.AddContentPart<GraphSyncPart>()
                .UseDisplayDriver<GraphSyncPartDisplayDriver>()
                .AddHandler<DataSyncPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphSyncPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddTransient<IContentPartDataSyncer, DataSyncPartDataSyncer>();
            services.AddTransient<IDataSyncPartDataSyncer, DataSyncPartDataSyncer>();
            services.AddSingleton<IIndexProvider, GraphSyncPartIndexProvider>();

            // orchestrators & orchestration handlers
            services.AddTransient<IDeleteOrchestrator, DeleteOrchestrator>();
            services.AddTransient<ISyncOrchestrator, SyncOrchestrator>();
            services.AddTransient<IContentTypeOrchestrator, ContentTypeOrchestrator>();
            services.AddTransient<IContentOrchestrationHandler, EventGridPublishingHandler>();

            // syncers
            services.AddTransient<IMergeDataSyncer, MergeDataSyncer>();
            services.AddTransient<IDeleteDataSyncer, DeleteDataSyncer>();
            services.AddTransient<IDeleteTypeDataSyncer, DeleteTypeDataSyncer>();
            services.AddTransient<ICloneDataSync, CloneDataSync>();
            services.AddTransient<IValidateAndRepairData, CosmosDbValidateAndRepairData>();
            services.AddTransient<IDataResyncer, DataResyncer>();
            services.AddTransient<IVisualiseDataSyncer, VisualiseDataSyncer>();

            services.AddTransient<ISyncNameProvider, SyncNameProvider>();
            services.AddTransient<ISyncNameProviderCSharpScriptGlobals, SyncNameProviderCSharpScriptGlobals>();
            services.AddTransient<IDataSyncValidationHelper, DataSyncValidationHelper>();
            services.AddTransient<IContentFieldsDataSyncer, ContentFieldsDataSyncer>();
            services.AddTransient<IBagPartEmbeddedContentItemsDataSyncer, CosmosDbBagPartEmbeddedContentItemsDataSyncer>();
            services.AddTransient<IFlowPartEmbeddedContentItemsDataSyncer, CosmosDbFlowPartEmbeddedContentItemsDataSyncer>();
            services.AddTransient<ITaxonomyPartEmbeddedContentItemsDataSyncer, CosmosDbTaxonomyPartEmbeddedContentItemsDataSyncer>();

            // content item syncers
            services.AddTransient<IContentItemDataSyncer, TaxonomyTermContentItemDataSyncer>();
            services.AddTransient<IContentItemDataSyncer, ContentItemDataSyncer>();

            // part syncers
            services.AddTransient<IContentPartDataSyncer, TitlePartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, BagPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, FlowPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, TaxonomyPartDataSyncer>();
            services.AddTransient<ITaxonomyPartDataSyncer, TaxonomyPartDataSyncer>();
            services.AddTransient<ITermPartDataSyncer, TermPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, EponymousPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, HtmlBodyPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, PublishLaterPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, UnpublishLaterPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, SitemapPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, AutoroutePartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, AliasPartDataSyncer>();
            services.AddTransient<IContentPartDataSyncer, FormPartDataSyncer>();

            // field syncers
            services.AddTransient<IContentFieldDataSyncer, TextFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, BooleanFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, NumericFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, HtmlFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, LinkFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, ContentPickerFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, DateTimeFieldDataSyncer>();
            services.AddTransient<IContentFieldDataSyncer, TaxonomyFieldDataSyncer>();

            // workflow activities
            services.AddActivity<AuditSyncIssuesTask, AuditSyncIssuesTaskDisplay>();

            // notifiers
            services.Replace(ServiceDescriptor.Scoped<IDataSyncNotifier, DataSyncNotifier>());
            services.Replace(ServiceDescriptor.Scoped<INotifier>(sp => sp.GetRequiredService<IDataSyncNotifier>()));

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

            // navigation
            services.AddScoped<INavigationProvider, AdminMenu>();

            //slack message publishing
            services.AddSlackMessagePublishing(_configuration, true);
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "DFC.ServiceTaxonomy.DataSync",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
