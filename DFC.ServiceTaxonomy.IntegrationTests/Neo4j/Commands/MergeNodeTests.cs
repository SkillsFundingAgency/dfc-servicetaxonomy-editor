using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Commands
{
    //todo: when acting call on ValidateResults on command

    [Collection("Graph Database Integration")]
    public class MergeNodeTests : GraphDatabaseIntegrationTest
    {
        public MergeNodeTests(GraphDatabaseCollectionFixture graphDatabaseCollectionFixture)
            : base(graphDatabaseCollectionFixture)
        {
        }

        //[Fact]
        //public async Task MergeNode_NoExistingNode_Test()
        //{
        //    const string nodeLabel = "testNode";
        //    const string idPropertyName = "testProperty";
        //    const string nodeVariable = "n";
        //    string idPropertyValue = Guid.NewGuid().ToString();

        //    //todo: is readonly enough, or should we clone? probably need to clone
        //    Dictionary<string,object> testProperties = new Dictionary<string, object>
        //        {{idPropertyName, idPropertyValue}};

        //    // act
        //    await MergeNode(nodeLabel, idPropertyName, testProperties);

        //    //todo: use reactive session?

        //    AssertResult(nodeVariable,new[]
        //    {
        //        new ExpectedNode
        //        {
        //            Labels = new[] {nodeLabel},
        //            Properties = testProperties
        //        }
        //    }, await AllNodes(nodeLabel, nodeVariable));
        //}

        //[Fact]
        //public async Task MergeNode_ExistingNode_SameProperties_Test()
        //{
        //    const string nodeLabel = "testNode";
        //    const string idPropertyName = "id";
        //    const string nodeVariable = "n";
        //    string idPropertyValue = Guid.NewGuid().ToString();

        //    //todo: arrange without using cut?
        //    await MergeNode(new MergeNodeCommand
        //    {
        //        NodeLabels = new HashSet<string> {nodeLabel},
        //        IdPropertyName = idPropertyName,
        //        Properties =
        //        {
        //            {idPropertyName, idPropertyValue},
        //            {"prop2", "prop2OriginalValue"}
        //        }
        //    });

        //    Dictionary<string,object> actProperties = new Dictionary<string, object>
        //    {
        //        {idPropertyName, idPropertyValue},
        //        {"prop2", "prop2NewValue"}
        //    };

        //    //act
        //    await MergeNode(nodeLabel, idPropertyName, actProperties);

        //    AssertResult(nodeVariable,new[]
        //    {
        //        new ExpectedNode
        //        {
        //            Labels = new[] {nodeLabel},
        //            Properties = actProperties
        //        }
        //    }, await AllNodes(nodeLabel, nodeVariable));
        //}

        ///// <summary>
        ///// MergeNode uses a map to replace all properties on the node.
        ///// </summary>
        //[Fact]
        //public async Task MergeNode_ExistingNode_DifferentProperties_Test()
        //{
        //    const string nodeLabel = "testNode";
        //    const string idPropertyName = "id";
        //    const string nodeVariable = "n";
        //    string idPropertyValue = Guid.NewGuid().ToString();

        //    await MergeNode(new MergeNodeCommand
        //    {
        //        NodeLabels = new HashSet<string> {nodeLabel},
        //        IdPropertyName = idPropertyName,
        //        Properties =
        //        {
        //            {idPropertyName, idPropertyValue},
        //            {"prop2", "prop2OriginalValue"},
        //            {"originalOnlyProp", "originalOnlyPropValue"}
        //        }
        //    });

        //    Dictionary<string,object> actProperties =
        //        new Dictionary<string, object>
        //        {
        //            {idPropertyName, idPropertyValue},
        //            {"prop2", "prop2NewValue"},
        //            {"newProp", "newPropValue"}
        //        };

        //    //act
        //    await MergeNode(nodeLabel, idPropertyName, actProperties);

        //    AssertResult(nodeVariable,new[]
        //    {
        //        new ExpectedNode
        //        {
        //            Labels = new[] {nodeLabel},
        //            Properties = actProperties
        //        }
        //    }, await AllNodes(nodeLabel, nodeVariable));
        //}

        //todo: multiple labels
    }
}
