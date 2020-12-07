using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    [CollectionDefinition("Graph Test Database Integration")]
    public class GraphDatabaseIntegrationCollection : ICollectionFixture<GraphDatabaseCollectionFixture>
    {
    }
}
