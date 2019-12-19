using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Generators;
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
            const string nodeVariable = "n";
            string idPropertyValue = Guid.NewGuid().ToString();

            //todo: is readonly enough, or should we clone? probably need to clone
            ReadOnlyDictionary<string, object> testProperties = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object> {{idPropertyName, idPropertyValue}});

            // act
            Query query = QueryGenerator.MergeNodes(nodeLabel, testProperties, idPropertyName);

            await _graphDatabase.RunWriteQueries(query);

            List<IRecord> actualRecords = await _graphDatabase.RunReadQuery(
                new Query($"match ({nodeVariable}:{nodeLabel}) return n"),
                r => r);

            // note: Records on a result cannot be accessed if the session or transaction where the result is created has been closed. (https://github.com/neo4j/neo4j-dotnet-driver)
            // Any query results obtained within a transaction function should be consumed within that function. Transaction functions can return values but these should be derived values rather than raw results. (https://neo4j.com/docs/driver-manual/1.7/sessions-transactions/#driver-transactions)
            //todo: ^^ should probably not ignore this!
            //todo: use reactive session?

            AssertResult(nodeVariable,new List<ExpectedNode>
            {
                new ExpectedNode
                {
                    Labels = new[] {nodeLabel},
                    Properties = testProperties
                }
            }, actualRecords);
        }

        [Fact]
        public async Task CreateNode_ExistingNode_Test()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "id";
            const string nodeVariable = "n";
            string idPropertyValue = Guid.NewGuid().ToString();

            ReadOnlyDictionary<string, object> arrangeProperties = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object>
                {
                    {idPropertyName, idPropertyValue},
                    {"prop2", "prop2OriginalValue"}
                });

            Query arrangeQuery = QueryGenerator.MergeNodes(nodeLabel, arrangeProperties, idPropertyName);
            await _graphDatabase.RunWriteQueries(arrangeQuery);

            ReadOnlyDictionary<string, object> actProperties = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object>
                {
                    {idPropertyName, idPropertyValue},
                    {"prop2", "prop2NewValue"}
                });

            //act
            Query actQuery = QueryGenerator.MergeNodes(nodeLabel, actProperties, idPropertyName);
            await _graphDatabase.RunWriteQueries(actQuery);

            List<IRecord> actualRecords = await _graphDatabase.RunReadQuery(
                new Query($"match ({nodeVariable}:{nodeLabel}) return n"),
                r => r);

            // note: Records on a result cannot be accessed if the session or transaction where the result is created has been closed. (https://github.com/neo4j/neo4j-dotnet-driver)
            // Any query results obtained within a transaction function should be consumed within that function. Transaction functions can return values but these should be derived values rather than raw results. (https://neo4j.com/docs/driver-manual/1.7/sessions-transactions/#driver-transactions)
            //todo: ^^ should probably not ignore this!
            //todo: use reactive session?

            AssertResult(nodeVariable,new List<ExpectedNode>
            {
                new ExpectedNode
                {
                    Labels = new[] {nodeLabel},
                    Properties = actProperties
                }
            }, actualRecords);
        }
    }
}
