using System;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Log;
using DFC.ServiceTaxonomy.Neo4j.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Neo4j.Driver;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphCluster(this IServiceCollection services, Action<Neo4jOptions>? setupAction)
        {
            services.AddSingleton<IGraphClusterBuilder, GraphClusterBuilder>();
            services.AddSingleton<IGraphCluster, GraphCluster>(provider => (GraphCluster)provider.GetRequiredService<IGraphClusterBuilder>().Build());
            services.AddTransient<ILogger, NeoLogger>();
            services.AddTransient<IMergeNodeCommand, MergeNodeCommand>();
            services.AddTransient<IDeleteNodeCommand, DeleteNodeCommand>();
            services.AddTransient<IDeleteNodesByTypeCommand, DeleteNodesByTypeCommand>();
            services.AddTransient<IReplaceRelationshipsCommand, ReplaceRelationshipsCommand>();
            services.AddTransient<IDeleteRelationshipsCommand, DeleteRelationshipsCommand>();
            services.AddTransient<ICustomCommand, CustomCommand>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
        }
    }
}
