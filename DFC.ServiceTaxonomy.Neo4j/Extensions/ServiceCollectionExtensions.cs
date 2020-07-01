using DFC.ServiceTaxonomy.Neo4j.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.Neo4j.Extensions
{
    //todo: best place for this file to live?
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphCluster(this IServiceCollection services)
        {
            services.AddSingleton<IGraphClusterBuilder, GraphClusterBuilder>();
            services.AddSingleton<IGraphCluster, GraphCluster>(provider => provider.GetRequiredService<IGraphClusterBuilder>().Build());
        }
    }
}
