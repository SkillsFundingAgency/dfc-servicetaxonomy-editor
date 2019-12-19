using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j
{
    [Collection("Graph Database Integration")]
    public class MergeNodeTests : GraphDatabaseIntegrationTest
    {
        public MergeNodeTests(GraphDatabaseCollectionFixture graphDatabaseCollectionFixture)
            : base(graphDatabaseCollectionFixture)
        {
        }

        [Fact]
        public async Task MergeNode_NoExistingNode_Test()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "testProperty";
            const string nodeVariable = "n";
            string idPropertyValue = Guid.NewGuid().ToString();

            //todo: is readonly enough, or should we clone? probably need to clone
            IReadOnlyDictionary<string,object> testProperties = new Dictionary<string, object>
                {{idPropertyName, idPropertyValue}};

            // act
            await MergeNode(nodeLabel, idPropertyName, testProperties);

            // note: Records on a result cannot be accessed if the session or transaction where the result is created has been closed. (https://github.com/neo4j/neo4j-dotnet-driver)
            // Any query results obtained within a transaction function should be consumed within that function. Transaction functions can return values but these should be derived values rather than raw results. (https://neo4j.com/docs/driver-manual/1.7/sessions-transactions/#driver-transactions)
            //todo: ^^ should probably not ignore this!
            //todo: use reactive session?

            AssertResult(nodeVariable,new[]
            {
                new ExpectedNode
                {
                    Labels = new[] {nodeLabel},
                    Properties = testProperties
                }
            }, await AllNodes(nodeLabel, nodeVariable));
        }

        [Fact]
        public async Task MergeNode_ExistingNode_SameProperties_Test()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "id";
            const string nodeVariable = "n";
            string idPropertyValue = Guid.NewGuid().ToString();

            await MergeNode(nodeLabel, idPropertyName,
                new Dictionary<string, object>
                {
                    {idPropertyName, idPropertyValue}, {"prop2", "prop2OriginalValue"}
                });

            IReadOnlyDictionary<string,object> actProperties = new Dictionary<string, object>
            {
                {idPropertyName, idPropertyValue},
                {"prop2", "prop2NewValue"}
            };

            //act
            await MergeNode(nodeLabel, idPropertyName, actProperties);

            AssertResult(nodeVariable,new[]
            {
                new ExpectedNode
                {
                    Labels = new[] {nodeLabel},
                    Properties = actProperties
                }
            }, await AllNodes(nodeLabel, nodeVariable));
        }

        /// <summary>
        /// MergeNode uses a map to replace all properties on the node.
        /// </summary>
        [Fact]
        public async Task MergeNode_ExistingNode_DifferentProperties_Test()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "id";
            const string nodeVariable = "n";
            string idPropertyValue = Guid.NewGuid().ToString();

            await MergeNode(nodeLabel, idPropertyName,
                new Dictionary<string, object>
                {
                    {idPropertyName, idPropertyValue},
                    {"prop2", "prop2OriginalValue"},
                    {"originalOnlyProp", "originalOnlyPropValue"}
                });

            IReadOnlyDictionary<string,object> actProperties =
                new Dictionary<string, object>
                {
                    {idPropertyName, idPropertyValue},
                    {"prop2", "prop2NewValue"},
                    {"newProp", "newPropValue"}
                };

            //act
            await MergeNode(nodeLabel, idPropertyName, actProperties);

            AssertResult(nodeVariable,new[]
            {
                new ExpectedNode
                {
                    Labels = new[] {nodeLabel},
                    Properties = actProperties
                }
            }, await AllNodes(nodeLabel, nodeVariable));
        }

        //todo: multiple labels
    }
}
