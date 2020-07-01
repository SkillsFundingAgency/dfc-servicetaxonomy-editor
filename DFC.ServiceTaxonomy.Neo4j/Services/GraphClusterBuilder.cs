using System;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public class GraphClusterBuilder : IGraphClusterBuilder
    {
        private readonly IOptionsMonitor<Neo4jConfiguration> _neo4JConfigurationOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public GraphClusterBuilder(
            IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions,
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _neo4JConfigurationOptions = neo4jConfigurationOptions;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public GraphCluster Build(Action<Neo4jConfiguration>? configure = null)
        {
            Neo4jConfiguration? currentConfig = _neo4JConfigurationOptions.CurrentValue;
            //todo: null
            //todo: ok to mutate returned CurrentValue?
            configure?.Invoke(currentConfig);

            //todo: neo4 doesn't encrypt by default (3 did), see https://neo4j.com/docs/driver-manual/current/client-applications/
            // TrustStrategy
            //o=>o.WithEncryptionLevel(EncryptionLevel.None));

            //todo: throw nice exceptions on missing config

            var neoEndpoints = currentConfig.Endpoints.Select(epc =>
                ActivatorUtilities.CreateInstance<NeoEndpoint>(_serviceProvider,
                epc.Name!, GraphDatabase.Driver(
                    epc.Uri, AuthTokens.Basic(epc.Username, epc.Password),
                    o => o.WithLogger(_logger))));

            var graphReplicaSets = currentConfig.ReplicaSets.Select(rsc =>
                new GraphReplicaSetLowLevel(
                    rsc.ReplicaSetName!,
                    rsc.Instances.Select((gic, index) =>
                        new Graph(
                            neoEndpoints.First(ep => ep.Name == gic.GraphName),
                            gic.GraphName!,
                            gic.DefaultGraph,
                            index))));

            return new GraphClusterLowLevel(graphReplicaSets);
        }
    }
}
