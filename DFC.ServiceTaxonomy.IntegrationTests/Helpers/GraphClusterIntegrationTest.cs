using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Neo4j.Driver;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterIntegrationTest
    {
        public const int NumberOfReplicasConfiguredForPublishedSet = 2;

        internal GraphClusterLowLevel GraphClusterLowLevel { get; }

        public ITestOutputHelper TestOutputHelper { get; }
        private readonly GraphClusterCollectionFixture _graphClusterCollectionFixture;

        internal GraphClusterIntegrationTest(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            _graphClusterCollectionFixture = graphClusterCollectionFixture;

            var neoEndpoints = _graphClusterCollectionFixture.Neo4jOptions.Endpoints
                .Where(epc => epc.Enabled)
                .Select(epc =>
                    new NeoEndpoint(epc.Name!,
                        GraphDatabase.Driver(
                            epc.Uri,
                            AuthTokens.Basic(epc.Username, epc.Password)),
//                            o => o.WithLogger(_logger))));
                        _graphClusterCollectionFixture.NLogLogger));

            var graphReplicaSets = _graphClusterCollectionFixture.Neo4jOptions.ReplicaSets
                .Select(rsc =>
                    new GraphReplicaSetLowLevel(rsc.ReplicaSetName!, ConstructGraphs(rsc, neoEndpoints)));

            GraphClusterLowLevel = new GraphClusterLowLevel(graphReplicaSets);
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

        protected void ReferenceCountTest(int parallelLoops)
        {
            const int replicaInstance = 0;

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, parallelLoops, (i, state) =>
            {
                TestOutputHelper.WriteLine($"Thread id: {Thread.CurrentThread.ManagedThreadId}");

                replicaSet.Disable(replicaInstance);
                replicaSet.Enable(replicaInstance);
            });

            int enabledInstanceCount = replicaSet.EnabledInstanceCount;
            Assert.Equal(NumberOfReplicasConfiguredForPublishedSet, enabledInstanceCount);
            Assert.True(replicaSet.IsEnabled(replicaInstance));
        }
    }
}
