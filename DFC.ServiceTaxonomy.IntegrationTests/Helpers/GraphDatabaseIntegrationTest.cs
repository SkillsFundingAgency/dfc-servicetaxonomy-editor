using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphDatabaseIntegrationTest : IAsyncLifetime, IDisposable
    {
        private readonly GraphDatabaseCollectionFixture _graphDatabaseCollectionFixture;
        private IGraphDatabaseTestRun? _graphDatabaseTestRun;

        protected IGraphDatabaseTestRun _graphDatabase => _graphDatabaseTestRun!;

        public GraphDatabaseIntegrationTest(GraphDatabaseCollectionFixture graphDatabaseCollectionFixture)
        {
            _graphDatabaseCollectionFixture = graphDatabaseCollectionFixture;
        }

        public async Task InitializeAsync()
        {
            _graphDatabaseTestRun = await _graphDatabaseCollectionFixture.GraphTestDatabase.StartTestRun();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public void Dispose() => _graphDatabaseTestRun?.Dispose();

        public void AssertResult(IEnumerable<ExpectedNode> expectedNodes, IEnumerable<IRecord> records)
        {

        }
    }
}
