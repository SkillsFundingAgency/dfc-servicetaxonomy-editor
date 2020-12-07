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
        public GraphClusterLowLevel_EnableDisableTests(
            GraphClusterCollectionFixture graphDatabaseCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphDatabaseCollectionFixture, testOutputHelper)
        {
        }

        [Fact]
        public void ReferenecCountTest()
        {
            const int replicaInstance = 0;

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, 100, (i, state) =>
            {
                TestOutputHelper.WriteLine($"Thread id: {Thread.CurrentThread.ManagedThreadId}");

                replicaSet.Disable(replicaInstance);
                replicaSet.Enable(replicaInstance);
            });

            int enabledInstanceCount = replicaSet.EnabledInstanceCount;
            Assert.Equal(0, enabledInstanceCount);
            Assert.False(replicaSet.IsEnabled(replicaInstance));
        }
    }
}
