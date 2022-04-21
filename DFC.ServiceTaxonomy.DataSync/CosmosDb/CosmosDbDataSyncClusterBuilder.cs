using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    public class CosmosDbDataSyncClusterBuilder : ICosmosDbDataSyncClusterBuilder
    {
        private readonly ILogger _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public CosmosDbDataSyncClusterBuilder(
            ILogger<CosmosDbDataSyncClusterBuilder> logger,
            ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        public IDataSyncCluster Build()
        {
            const string publishedKey = "published";
            const string previewKey = "preview";

            return new CosmosDbDataSyncClusterLowLevel(new List<DataSyncReplicaSetLowLevel>
            {
                new DataSyncReplicaSetLowLevel(publishedKey, new List<Models.DataSync>()
                {
                    new Models.DataSync(new CosmosDbEndpoint(_cosmosDbService, publishedKey, _logger), publishedKey, true, 0, _logger)
                }, _logger),
                new DataSyncReplicaSetLowLevel(previewKey, new List<Models.DataSync>()
                {
                    new Models.DataSync(new CosmosDbEndpoint(_cosmosDbService, previewKey, _logger), previewKey, false, 0, _logger)
                }, _logger)
            });
        }
    }
}
