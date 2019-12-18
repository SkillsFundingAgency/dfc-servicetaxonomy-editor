using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Generators;
using KellermanSoftware.CompareNetObjects;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j
{
    [Collection("Graph Database Integration")]
    public class MergeNodesTests : GraphDatabaseIntegrationTest
    {
        public MergeNodesTests(GraphDatabaseCollectionFixture graphDatabaseCollectionFixture)
            : base(graphDatabaseCollectionFixture)
        {
        }

        [Fact]
        public async Task CreateNode_NoExistingNode_Test()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "testProperty";
            string idPropertyValue = Guid.NewGuid().ToString();

            //todo: is readonly enough, or should we clone? probably need to clone
            ReadOnlyDictionary<string, object> testProperties = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object> {{idPropertyName, idPropertyValue}});

            Query query = QueryGenerator.MergeNodes(nodeLabel, testProperties, idPropertyName);

            //todo: create test NeoGraphDatabase that keeps trans open, then rollbacks after test?
            await _graphDatabase.RunWriteQueries(query);

            //todo: we could convert to INode here, but if conversion fails, we won't get an assert failing
            List<IRecord> actualRecords = await _graphDatabase.RunReadQuery(
                new Query($"match (n:{nodeLabel}) return n"),
                r => r);

            // note: Records on a result cannot be accessed if the session or transaction where the result is created has been closed. (https://github.com/neo4j/neo4j-dotnet-driver)
            // Any query results obtained within a transaction function should be consumed within that function. Transaction functions can return values but these should be derived values rather than raw results. (https://neo4j.com/docs/driver-manual/1.7/sessions-transactions/#driver-transactions)
            //todo: ^^ should probably not ignore this!
            //todo: use reactive session?

            //todo: helper that accepts e.g. IEnumerable<ExpectedNode> expectedNodes

            Assert.Single(actualRecords);

            var record = actualRecords.First();
            Assert.Single(record.Values);

            var value = record.Values.First();

            // meta assert: check we have the correct variable
            Assert.Equal("n", value.Key);

            INode node = value.Value.As<INode>();
            Assert.NotNull(node);

            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.IgnoreProperty<ExpectedNode>(n => n.Id);
            compareLogic.Config.IgnoreObjectTypes = true;
            compareLogic.Config.SkipInvalidIndexers = true;
            compareLogic.Config.MaxDifferences = 10;

            INode expectedNode = new ExpectedNode
            {
                Labels = new[] {nodeLabel},
                Properties = testProperties
            };

            ComparisonResult comparisonResult = compareLogic.Compare(expectedNode, node);

            Assert.True(comparisonResult.AreEqual, $"Returned node different to expected: {comparisonResult.DifferencesString}");
        }

        [Fact]
        public async Task ReplaceNodeTest()
        {
            await _graphDatabase.RunWriteQueries(new Query("match (n) return n limit 1"));
        }
    }
}
