using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbGraphClusterBuilder : ICosmosDbGraphClusterBuilder
    {
        private readonly IOptionsMonitor<CosmosDbOptions> _configurationOptions;
        private readonly ILogger _logger;

        public CosmosDbGraphClusterBuilder(
            IOptionsMonitor<CosmosDbOptions> configurationOptions,
            ILogger<CosmosDbGraphClusterBuilder> logger)
        {
            _configurationOptions = configurationOptions;
            _logger = logger;
        }

        public IGraphCluster Build(Action<CosmosDbOptions>? configure = null)
        {
            CosmosDbOptions? currentConfig = _configurationOptions.CurrentValue;
            configure?.Invoke(currentConfig);

            if (!currentConfig.Endpoints.Any())
                throw new GraphClusterConfigurationErrorException("No endpoints configured.");

            const string publishedKey = "published";
            const string previewKey = "preview";
            var endpoint = currentConfig.Endpoints[0];

            return new CosmosDbGraphClusterLowLevel(new List<GraphReplicaSetLowLevel>
            {
                new GraphReplicaSetLowLevel(publishedKey, new List<Graph>()
                {
                    new Graph(new CosmosDbEndpoint(endpoint), publishedKey, true, 0, _logger)
                }, _logger),
                new GraphReplicaSetLowLevel(previewKey, new List<Graph>()
                {
                    new Graph(new CosmosDbEndpoint(endpoint), previewKey, false, 0, _logger)
                }, _logger)
            });
        }
    }
}
