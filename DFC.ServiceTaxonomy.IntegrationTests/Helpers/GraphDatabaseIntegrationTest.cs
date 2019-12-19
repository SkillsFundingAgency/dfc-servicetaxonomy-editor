using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Generators;
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

        //todo: params for properties?
        protected async Task<ReadOnlyDictionary<string, object>> MergeNode(string label, string idPropertyName, IDictionary<string,object> properties)
        {
            ReadOnlyDictionary<string, object> readOnlyProperties = new ReadOnlyDictionary<string, object>(properties);

            Query arrangeQuery = QueryGenerator.MergeNode(label, idPropertyName, readOnlyProperties);
            await _graphDatabase.RunWriteQueries(arrangeQuery);

            return readOnlyProperties;
        }

        /// <summary>
        /// Doesn't support cartesian products!
        /// </summary>
        protected void AssertResult(string variableName, List<ExpectedNode> expectedNodes, IEnumerable<IRecord> actualRecords)
        {
            var actualVariableRecords = actualRecords.Where(r => r.Keys.Contains(variableName)).ToArray();
            int countActualVariableRecords = actualVariableRecords.Length;

            if (countActualVariableRecords == 0)
            {
                Assert.True(expectedNodes.Count == 0, "Expected at least one node, but none returned (no results for variable)");

                // expecting empty result, got empty result
                return;
            }

            Assert.True(countActualVariableRecords == 1, "This assert doesn't support cartesian products. Did you mean to return a cartesian product?");

            IRecord variableRecord = actualVariableRecords.First();

            IEnumerable<object> actualNodes = variableRecord.Values.Select(v => v.Value);

            ComparisonResult comparisonResult = _graphDatabaseCollectionFixture.CompareLogic.Compare(expectedNodes, actualNodes);

            Assert.True(comparisonResult.AreEqual, $"Returned nodes different to expected: {comparisonResult.DifferencesString}");
        }
    }
}
