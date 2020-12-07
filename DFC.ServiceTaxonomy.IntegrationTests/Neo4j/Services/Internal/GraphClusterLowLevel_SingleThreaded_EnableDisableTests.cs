using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    // assertions are in the base class methods
    #pragma warning disable S2699

    [Collection("GraphCluster Integration")]
    public class GraphClusterLowLevel_SingleThreaded_EnableDisableTests : GraphClusterIntegrationTest
    {
        public GraphClusterLowLevel_SingleThreaded_EnableDisableTests(
            GraphClusterCollectionFixture graphDatabaseCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphDatabaseCollectionFixture, testOutputHelper)
        {
        }

        //todo: need proper tear down, as multi-threaded test is breaking single threaded test, 2 parts:
        // 1) stop tests running in parallel with each other
        // 2) ensure tests start with a clean graph cluster

        [Fact]
        public void SingleThreaderReferenceCountTest()
        {
            ReferenceCountTest(1);
        }
    }

    #pragma warning restore S2699
}
