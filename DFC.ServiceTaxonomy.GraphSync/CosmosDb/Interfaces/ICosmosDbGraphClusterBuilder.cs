using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces
{
    public interface ICosmosDbGraphClusterBuilder
    {
        IGraphCluster Build();
    }
}
