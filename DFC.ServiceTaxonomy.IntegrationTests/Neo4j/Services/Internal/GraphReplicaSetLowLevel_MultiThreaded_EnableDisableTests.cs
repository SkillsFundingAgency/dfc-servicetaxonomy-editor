#if REPLICA_DISABLING_NET5_ONLY

using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    // assertions are in the base class methods
#pragma warning disable S2699

    [Collection("GraphCluster Integration")]
    public class GraphReplicaSetLowLevel_MultiThreaded_EnableDisableTests : GraphClusterIntegrationTest
    {
        public GraphReplicaSetLowLevel_MultiThreaded_EnableDisableTests(
            GraphClusterCollectionFixture graphDatabaseCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphDatabaseCollectionFixture, testOutputHelper)
        {
        }

        [Fact]
        public void MultiThreadedReferenceCountTest()
        {
            ReferenceCountTest(100);
        }
    }

#pragma warning restore S2699
}
#endif
