using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    [CollectionDefinition("Graph Test Database Integration")]
    public sealed class GraphTestDatabaseIntegrationCollection : ICollectionFixture<GraphTestDatabaseCollectionFixture>
    {
    }
}
