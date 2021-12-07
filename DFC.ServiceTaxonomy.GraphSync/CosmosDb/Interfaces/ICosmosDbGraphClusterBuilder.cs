using System;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces
{
    public interface ICosmosDbGraphClusterBuilder
    {
        IGraphCluster Build(Action<CosmosDbOptions>? configure = null);
    }
}
