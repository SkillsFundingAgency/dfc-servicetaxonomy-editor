using System;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphCluster(this IServiceCollection services, Action<CosmosDbOptions>? setupAction)
        {
            services.AddSingleton<ICosmosDbGraphClusterBuilder, CosmosDbGraphClusterBuilder>();
            services.AddSingleton<IGraphCluster, CosmosDbGraphCluster>(
                provider =>
                {
                    var clusterBuilder = provider.GetRequiredService<ICosmosDbGraphClusterBuilder>().Build();
                    return (CosmosDbGraphCluster)clusterBuilder;
                });
            services.AddTransient<IMergeNodeCommand, CosmosDbMergeNodeCommand>();
            services.AddTransient<IDeleteNodeCommand, CosmosDbDeleteNodeCommand>();
            services.AddTransient<IDeleteNodesByTypeCommand, CosmosDbDeleteNodesByTypeCommand>();
            services.AddTransient<IReplaceRelationshipsCommand, CosmosDbReplaceRelationshipsCommand>();
            services.AddTransient<IDeleteRelationshipsCommand, CosmosDbDeleteRelationshipsCommand>();
            services.AddTransient<ICustomCommand, CustomCommand>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
        }
    }
}
