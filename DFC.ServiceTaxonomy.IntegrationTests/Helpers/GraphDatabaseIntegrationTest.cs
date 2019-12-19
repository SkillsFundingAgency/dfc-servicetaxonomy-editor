using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
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

        /// <summary>
        /// Doesn't support cartesian products!
        /// </summary>
        protected void AssertResult(string variableName, List<ExpectedNode> expectedNodes, List<IRecord> actualRecords)
        {
            var actualVariableRecords = actualRecords.Where(r => r.Keys.Contains(variableName));
            int countActualVariableRecords = actualVariableRecords.Count();

            if (countActualVariableRecords == 0)
            {
                Assert.True(expectedNodes.Count == 0, "Expected at least one node, but none returned (no results for variable)");

                // expecting empty result, got empty result
                return;
            }

            Assert.True(countActualVariableRecords == 1, "This assert doesn't support cartesian products. Did you mean to return a cartesian product?");

            var variableRecord = actualVariableRecords.First();

            var actualNodes = variableRecord.Values.Select(v => v.Value);

            //todo: comparelogic is thread safe: create one as part of collection fixture
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.IgnoreProperty<ExpectedNode>(n => n.Id);
            compareLogic.Config.IgnoreObjectTypes = true;
            compareLogic.Config.SkipInvalidIndexers = true;
            compareLogic.Config.MaxDifferences = 10;

            ComparisonResult comparisonResult = compareLogic.Compare(expectedNodes, actualNodes);

            Assert.True(comparisonResult.AreEqual, $"Returned nodes different to expected: {comparisonResult.DifferencesString}");
        }
    }
}
