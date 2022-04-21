using DFC.ServiceTaxonomy.DataSync.Interfaces;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb.Interfaces
{
    public interface ICosmosDbDataSyncClusterBuilder
    {
        IDataSyncCluster Build();
    }
}
