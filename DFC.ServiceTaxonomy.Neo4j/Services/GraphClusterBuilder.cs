using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public class GraphClusterBuilder : IGraphClusterBuilder
    {
        private readonly IOptionsMonitor<Neo4jOptions> _neo4JConfigurationOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _neoLogger;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public GraphClusterBuilder(
            IOptionsMonitor<Neo4jOptions> neo4jConfigurationOptions,
            IServiceProvider serviceProvider,
            ILogger neoLogger,
            Microsoft.Extensions.Logging.ILogger<GraphClusterBuilder> logger)
        {
            _neo4JConfigurationOptions = neo4jConfigurationOptions;
            _serviceProvider = serviceProvider;
            _neoLogger = neoLogger;
            _logger = logger;
        }

        public IGraphCluster Build(Action<Neo4jOptions>? configure = null)
        {
            Neo4jOptions? currentConfig = _neo4JConfigurationOptions.CurrentValue;
            //todo: ok to mutate returned CurrentValue?
            //todo: pass back builder
            configure?.Invoke(currentConfig);

            //todo: neo4 doesn't encrypt by default (3 did), see https://neo4j.com/docs/driver-manual/current/client-applications/
            // TrustStrategy
            //o=>o.WithEncryptionLevel(EncryptionLevel.None));

            if (!currentConfig.Endpoints.Any())
                throw new GraphClusterConfigurationErrorException("No endpoints configured.");

            var neoEndpoints = currentConfig.Endpoints
                .Where(epc => epc.Enabled)
                .Select(epc =>
                    ActivatorUtilities.CreateInstance<NeoEndpoint>(
                        _serviceProvider,
                        epc.Name!,
                        GraphDatabase.Driver(
                            epc.Uri,
                            AuthTokens.Basic(epc.Username, epc.Password),
                            o => o.WithLogger(_neoLogger))));

            if (!currentConfig.ReplicaSets.Any())
                throw new GraphClusterConfigurationErrorException("No replica sets configured.");

            var graphReplicaSets = currentConfig.ReplicaSets
                .Select(rsc =>
                    new GraphReplicaSetLowLevel(rsc.ReplicaSetName!, ConstructGraphs(rsc, neoEndpoints)));

            return new GraphClusterLowLevel(graphReplicaSets, _logger);
            //return ActivatorUtilities.CreateInstance<GraphClusterLowLevel>(_serviceProvider, graphReplicaSets);
        }

        private IEnumerable<Graph> ConstructGraphs(ReplicaSetConfiguration replicaSetConfiguration, IEnumerable<NeoEndpoint> neoEndpoints)
        {
            return replicaSetConfiguration.GraphInstances
                .Where(gic => gic.Enabled)
                .Select((gic, index) =>
                    ActivatorUtilities.CreateInstance<Graph>(_serviceProvider,
                    neoEndpoints.First(ep => ep.Name == gic.Endpoint),
                        gic.GraphName!,
                        gic.DefaultGraph,
                        index));
        }
    }
}
