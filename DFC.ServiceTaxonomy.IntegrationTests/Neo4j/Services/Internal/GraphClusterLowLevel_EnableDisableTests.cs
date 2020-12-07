using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    [Collection("GraphCluster Integration")]
    public class GraphClusterLowLevel_EnableDisableTests : GraphClusterIntegrationTest
    {
        public const int NumberOfReplicasConfiguredForPublishedSet = 2;

        public GraphClusterLowLevel_EnableDisableTests(
            GraphClusterCollectionFixture graphDatabaseCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphDatabaseCollectionFixture, testOutputHelper)
        {
        }

        //todo: need proper tear down, as multi-threaded test is breaking single threaded test

        //[Theory]
        // single-threaded test
        // [InlineData(1)]
        // multi-threaded test
        // [InlineData(100)]
        public void ReferenceCountTest(int parallelLoops)
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
