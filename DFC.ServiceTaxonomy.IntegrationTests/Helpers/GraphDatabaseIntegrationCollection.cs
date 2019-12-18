using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    [CollectionDefinition("Graph Database Integration")]
    public class GraphDatabaseIntegrationCollection : ICollectionFixture<GraphDatabaseFixture>
    {
    }
}
