using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using KellermanSoftware.CompareNetObjects;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphTestDatabaseIntegrationTest : IAsyncLifetime, IDisposable
    {
        private readonly GraphTestDatabaseCollectionFixture _graphTestDatabaseCollectionFixture;
        private IGraphDatabaseTestRun? _graphDatabaseTestRun;

        protected IGraphDatabaseTestRun _graphDatabase => _graphDatabaseTestRun!;

        protected GraphTestDatabaseIntegrationTest(GraphTestDatabaseCollectionFixture graphTestDatabaseCollectionFixture)
        {
            _graphTestDatabaseCollectionFixture = graphTestDatabaseCollectionFixture;
        }

        public async Task InitializeAsync()
        {
            _graphDatabaseTestRun = await _graphTestDatabaseCollectionFixture.GraphTestDatabase.StartTestRun();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _graphDatabaseTestRun?.Dispose();
        }

        protected async Task<long> MergeNode(MergeNodeCommand mergeNodeCommand)
        {
            List<long> result = await _graphDatabase.RunWriteQuery(mergeNodeCommand,
                r => r.Values.First().Value.As<long>());
            return result.First();
        }

        protected async Task<long> MergeNode(string label, string idPropertyName, IDictionary<string, object> properties)
        {
            return await MergeNode(new[] {label}, idPropertyName, properties);
        }

        protected async Task<long> MergeNode(IEnumerable<string> labels, string idPropertyName,
            IDictionary<string,object> properties)
        {
            return await MergeNode(new MergeNodeCommand
            {
                NodeLabels = new HashSet<string>(labels),
                IdPropertyName = idPropertyName,
                Properties = properties
            });
        }

        protected async Task<List<IRecord>> AllNodes(string label, string variableName)
        {
            return await _graphDatabase.RunReadQuery(
                new Query($"match ({variableName}:{label}) return {variableName}"),
                r => r);
        }

        protected async Task<List<IRecord>> AllRelationships(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue,
            string relationshipType,
            string destNodeLabel,
            string variableName)
        {
            return await _graphDatabase.RunReadQuery(
                new Query($"match (:{sourceNodeLabel} {{{sourceIdPropertyName}:'{sourceIdPropertyValue}'}})-[{variableName}:{relationshipType}]->(:{destNodeLabel}) return {variableName}"),
                r => r);
        }

        /// <summary>
        /// Doesn't support cartesian products!
        /// </summary>
        protected void AssertResult(string variableName, IEnumerable<object> expected, IEnumerable<IRecord> actualRecords)
        {
            var actualVariableRecords = actualRecords.Where(r => r.Keys.Contains(variableName)).ToArray();
            int countActualVariableRecords = actualVariableRecords.Length;

            if (countActualVariableRecords == 0)
            {
                Assert.True(!expected.Any(), "Expected at least one node, but none returned (no results for variable)");

                // expecting empty result, got empty result
                return;
            }

            Assert.True(countActualVariableRecords == 1, "This assert doesn't support cartesian products. Did you mean to return a cartesian product?");

            IRecord variableRecord = actualVariableRecords.First();

            IEnumerable<object> actualNodes = variableRecord.Values.Select(v => v.Value);

            ComparisonResult comparisonResult = _graphTestDatabaseCollectionFixture.CompareLogic.Compare(expected, actualNodes);

            Assert.True(comparisonResult.AreEqual, $"Returned nodes different to expected: {comparisonResult.DifferencesString}");
        }
    }
}
