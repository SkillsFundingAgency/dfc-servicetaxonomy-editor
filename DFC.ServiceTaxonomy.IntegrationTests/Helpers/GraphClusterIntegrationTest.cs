using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterIntegrationTest //: IAsyncLifetime
    {
        public ITestOutputHelper TestOutputHelper { get; }
        private readonly GraphClusterCollectionFixture _graphClusterCollectionFixture;

        internal GraphClusterIntegrationTest(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
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
