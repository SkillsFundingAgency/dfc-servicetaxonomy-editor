using DFC.ServiceTaxonomy.Neo4j.Services.Internal;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterIntegrationTest //: IAsyncLifetime
    {
        private readonly GraphClusterCollectionFixture _graphClusterCollectionFixture;

        internal GraphClusterIntegrationTest(GraphClusterCollectionFixture graphClusterCollectionFixture)
        {
            _graphClusterCollectionFixture = graphClusterCollectionFixture;
        }

        internal GraphClusterLowLevel GraphClusterLowLevel
        {
            get
            {
                return _graphClusterCollectionFixture.GraphClusterLowLevel;
            }
        }

        // public Task InitializeAsync()
        // {
        //     return Task.CompletedTask;
        // }
        //
        // public Task DisposeAsync()
        // {
        //     return Task.CompletedTask;
        // }
    }
}
