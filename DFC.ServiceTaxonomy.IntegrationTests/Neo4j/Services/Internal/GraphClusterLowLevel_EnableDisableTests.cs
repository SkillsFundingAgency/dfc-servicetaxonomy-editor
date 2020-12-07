using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    [Collection("GraphCluster Integration")]
    public class GraphClusterLowLevel_EnableDisableTests : GraphClusterIntegrationTest
    {
        public GraphClusterLowLevel_EnableDisableTests(GraphClusterCollectionFixture graphDatabaseCollectionFixture)
            : base(graphDatabaseCollectionFixture)
        {
        }

        // [Fact]
        // public void X()
        // {
        // }
    }
}
