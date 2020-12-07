using System.Collections.Generic;
using System.IO;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterCollectionFixture //: IDisposable
    {
        internal GraphClusterLowLevel GraphClusterLowLevel { get; }
        internal ILogger<NeoEndpoint> NLogLogger { get; }
        // public CompareLogic CompareLogic { get; }

        public GraphClusterCollectionFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            Neo4jOptions neo4jOptions = configuration.GetSection("Neo4j").Get<Neo4jOptions>();

            NLogLogger = A.Fake<ILogger<NeoEndpoint>>();

            if (!neo4jOptions.Endpoints.Any())
                throw new GraphClusterConfigurationErrorException("No endpoints configured.");

            var neoEndpoints = neo4jOptions.Endpoints
                .Where(epc => epc.Enabled)
                .Select(epc =>
                    new NeoEndpoint(epc.Name!,
                        GraphDatabase.Driver(
                            epc.Uri,
                            AuthTokens.Basic(epc.Username, epc.Password)),
//                            o => o.WithLogger(_logger))));
                        NLogLogger));

            if (!neo4jOptions.ReplicaSets.Any())
                throw new GraphClusterConfigurationErrorException("No replica sets configured.");

            var graphReplicaSets = neo4jOptions.ReplicaSets
                .Select(rsc =>
                    new GraphReplicaSetLowLevel(rsc.ReplicaSetName!, ConstructGraphs(rsc, neoEndpoints)));

            GraphClusterLowLevel = new GraphClusterLowLevel(graphReplicaSets);

            // CompareLogic = new CompareLogic();
            // CompareLogic.Config.IgnoreProperty<ExpectedNode>(n => n.Id);
            // CompareLogic.Config.IgnoreProperty<ExpectedRelationship>(r => r.Id);
            // CompareLogic.Config.IgnoreObjectTypes = true;
            // CompareLogic.Config.SkipInvalidIndexers = true;
            // CompareLogic.Config.MaxDifferences = 10;
        }

        private IEnumerable<Graph> ConstructGraphs(ReplicaSetConfiguration replicaSetConfiguration, IEnumerable<NeoEndpoint> neoEndpoints)
        {
            return replicaSetConfiguration.GraphInstances
                .Where(gic => gic.Enabled)
                .Select((gic, index) =>
                    new Graph(
                        neoEndpoints.First(ep => ep.Name == gic.Endpoint),
                        gic.GraphName!,
                        gic.DefaultGraph,
                        index));
        }

        // public void Dispose()
        // {
        //     GraphCluster?.Dispose();
        // }
    }
}
