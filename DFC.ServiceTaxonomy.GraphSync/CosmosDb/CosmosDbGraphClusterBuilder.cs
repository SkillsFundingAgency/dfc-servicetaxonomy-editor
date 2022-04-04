using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbGraphClusterBuilder : ICosmosDbGraphClusterBuilder
    {
        private readonly ILogger _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public CosmosDbGraphClusterBuilder(
            ILogger<CosmosDbGraphClusterBuilder> logger,
            ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        public IGraphCluster Build()
        {
            const string publishedKey = "published";
            const string previewKey = "preview";

            return new CosmosDbGraphClusterLowLevel(new List<GraphReplicaSetLowLevel>
            {
                new GraphReplicaSetLowLevel(publishedKey, new List<Graph>()
                {
                    new Graph(new CosmosDbEndpoint(_cosmosDbService, publishedKey, _logger), publishedKey, true, 0, _logger)
                }, _logger),
                new GraphReplicaSetLowLevel(previewKey, new List<Graph>()
                {
                    new Graph(new CosmosDbEndpoint(_cosmosDbService, previewKey, _logger), previewKey, false, 0, _logger)
                }, _logger)
            });
        }
    }
}
