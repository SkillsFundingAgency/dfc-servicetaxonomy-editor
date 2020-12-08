using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterIntegrationTest
    {
        internal const int NumberOfReplicasConfiguredForPublishedSet = 2;

        internal GraphClusterLowLevel GraphClusterLowLevel { get; }
        internal ITestOutputHelper TestOutputHelper { get; }

        internal GraphClusterIntegrationTest(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            GraphClusterCollectionFixture graphClusterCollectionFixture1 = graphClusterCollectionFixture;

            var neoEndpoints = graphClusterCollectionFixture1.Neo4jOptions.Endpoints
                .Where(epc => epc.Enabled)
                .Select(epc =>
                    CreateFakeNeoEndpoint(epc.Name!));
                    // new NeoEndpoint(epc.Name!,
                    //     GraphDatabase.Driver(
                    //         epc.Uri,
                    //         AuthTokens.Basic(epc.Username, epc.Password)),
                            // o => o.WithLogger(_logger))));
                        //graphClusterCollectionFixture1.NLogLogger);

            var graphReplicaSets = graphClusterCollectionFixture1.Neo4jOptions.ReplicaSets
                .Select(rsc =>
                    new GraphReplicaSetLowLevel(rsc.ReplicaSetName!, ConstructGraphs(rsc, neoEndpoints)));

            GraphClusterLowLevel = new GraphClusterLowLevel(graphReplicaSets);
        }

        private INeoEndpoint CreateFakeNeoEndpoint(string name)
        {
            var neoEndpoint = A.Fake<INeoEndpoint>();
            A.CallTo(() => neoEndpoint.Name)
                .Returns(name);
            return neoEndpoint;
        }

        private IEnumerable<Graph> ConstructGraphs(
            ReplicaSetConfiguration replicaSetConfiguration,
            IEnumerable<INeoEndpoint> neoEndpoints)
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
