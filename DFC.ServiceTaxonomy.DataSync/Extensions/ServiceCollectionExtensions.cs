using DFC.ServiceTaxonomy.DataSync.CosmosDb;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Commands;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.DataSync.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDataSyncCluster(this IServiceCollection services)
        {
            services.AddSingleton<ICosmosDbDataSyncClusterBuilder, CosmosDbDataSyncClusterBuilder>();
            services.AddSingleton<IDataSyncCluster, CosmosDbDataSyncCluster>(
                provider =>
                {
                    var clusterBuilder = provider.GetRequiredService<ICosmosDbDataSyncClusterBuilder>().Build();
                    return (CosmosDbDataSyncCluster)clusterBuilder;
                });
            services.AddTransient<IMergeNodeCommand, CosmosDbMergeNodeCommand>();
            services.AddTransient<IDeleteNodeCommand, CosmosDbDeleteNodeCommand>();
            services.AddTransient<IDeleteNodesByTypeCommand, CosmosDbDeleteNodesByTypeCommand>();
            services.AddTransient<IReplaceRelationshipsCommand, CosmosDbReplaceRelationshipsCommand>();
            services.AddTransient<IDeleteRelationshipsCommand, CosmosDbDeleteRelationshipsCommand>();
            services.AddTransient<ICustomCommand, CustomCommand>();
        }
    }
}
