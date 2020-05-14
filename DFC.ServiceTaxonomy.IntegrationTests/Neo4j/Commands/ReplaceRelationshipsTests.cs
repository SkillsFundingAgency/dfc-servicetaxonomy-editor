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
    public class ReplaceRelationshipsTests : GraphDatabaseIntegrationTest
    {
        public ReplaceRelationshipsTests(GraphDatabaseCollectionFixture graphDatabaseCollectionFixture)
            : base(graphDatabaseCollectionFixture)
        {
        }

        //[Fact]
        //public async Task ReplaceRelationships_CreateSingleNewRelationship_NoExistingRelationships_Test()
        //{
        //    const string sourceNodeLabel = "sourceNodeLabel";
        //    const string sourceIdPropertyName = "sourceId";
        //    string sourceIdPropertyValue = Guid.NewGuid().ToString();

        //    const string destNodeLabel = "destNodeLabel";
        //    const string destIdPropertyName = "destId";
        //    string destIdPropertyValue = Guid.NewGuid().ToString();

        //    const string relationshipType = "relationshipType";
        //    const string relationshipVariable = "r";

        //    //todo: arrange without any of the cut?
        //    // create source node to create relationship from
        //    long sourceNodeId = await MergeNode(sourceNodeLabel, sourceIdPropertyName,
        //        new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

        //    // create destination node to create relationship to
        //    long destNodeId = await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue}});

        //    // act
        //    var command = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };
        //    command.AddRelationshipsTo(
        //        relationshipType,
        //        new[] {destNodeLabel},
        //        destIdPropertyName,
        //        destIdPropertyValue);
        //    await _graphDatabase.RunWriteQueries(command);

        //    AssertResult(relationshipVariable,new[]
        //    {
        //        new ExpectedRelationship
        //        {
        //            Type = relationshipType,
        //            StartNodeId = sourceNodeId,
        //            EndNodeId = destNodeId,
        //            Properties = new Dictionary<string, object>()
        //        }
        //    }, await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
        //        relationshipType, destNodeLabel, relationshipVariable));
        //}

        //[Fact]
        //public async Task ReplaceRelationships_CreateSingleNewRelationship_ExistingRelationship_Test()
        //{
        //    const string sourceNodeLabel = "sourceNodeLabel";
        //    const string sourceIdPropertyName = "sourceId";
        //    string sourceIdPropertyValue = Guid.NewGuid().ToString();

        //    const string destNodeLabel = "destNodeLabel";
        //    const string destIdPropertyName = "destId";
        //    string destIdPropertyValue = Guid.NewGuid().ToString();

        //    string preexistingDestIdPropertyValue = Guid.NewGuid().ToString();

        //    const string relationshipType = "relationshipType";
        //    const string relationshipVariable = "r";

        //    // create source node to create relationship from
        //    long sourceNodeId = await MergeNode(sourceNodeLabel, sourceIdPropertyName,
        //        new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

        //    // create destination node for preexisting relationship
        //    await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, preexistingDestIdPropertyValue}});

        //    // create destination node to create new relationship to
        //    long destNodeId = await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue}});

        //    // create pre-existing relationships
        //    var preexistingQuery = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };

        //    preexistingQuery.AddRelationshipsTo(relationshipType, new [] {destNodeLabel}, destIdPropertyName, destIdPropertyValue);

        //    await _graphDatabase.RunWriteQueries(preexistingQuery);

        //    // act
        //    //todo: change dest node?
        //    var query = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };

        //    query.AddRelationshipsTo(relationshipType, new [] {destNodeLabel}, destIdPropertyName, destIdPropertyValue);

        //    await _graphDatabase.RunWriteQueries(query);

        //    AssertResult(relationshipVariable,new[]
        //    {
        //        new ExpectedRelationship
        //        {
        //            Type = relationshipType,
        //            StartNodeId = sourceNodeId,
        //            EndNodeId = destNodeId,
        //            Properties = new Dictionary<string, object>()
        //        }
        //    }, await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
        //        relationshipType, destNodeLabel, relationshipVariable));
        //}

        //[Fact]
        //public async Task ReplaceRelationships_CreateNoNewRelationship_NoExistingRelationships_Test()
        //{
        //    const string sourceNodeLabel = "sourceNodeLabel";
        //    const string sourceIdPropertyName = "sourceId";
        //    string sourceIdPropertyValue = Guid.NewGuid().ToString();

        //    const string destNodeLabel = "destNodeLabel";
        //    const string destIdPropertyName = "destId";
        //    string destIdPropertyValue = Guid.NewGuid().ToString();

        //    const string relationshipType = "relationshipType";
        //    const string relationshipVariable = "r";

        //    //todo: arrange without any of the cut?
        //    // create source node to create relationship from
        //    await MergeNode(sourceNodeLabel, sourceIdPropertyName,
        //        new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

        //    // create destination node to create relationship to
        //    await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue}});

        //    // act
        //    var query = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };

        //    query.AddRelationshipsTo(relationshipType, new [] {destNodeLabel}, destIdPropertyName);

        //    await _graphDatabase.RunWriteQueries(query);

        //    AssertResult(relationshipVariable,new ExpectedRelationship[0],
        //        await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
        //        relationshipType, destNodeLabel, relationshipVariable));
        //}

        //[Fact]
        //public async Task ReplaceRelationships_CreateNoNewRelationship_ExistingRelationship_Test()
        //{
        //    const string sourceNodeLabel = "sourceNodeLabel";
        //    const string sourceIdPropertyName = "sourceId";
        //    string sourceIdPropertyValue = Guid.NewGuid().ToString();

        //    const string destNodeLabel = "destNodeLabel";
        //    const string destIdPropertyName = "destId";
        //    string destIdPropertyValue = Guid.NewGuid().ToString();

        //    string preexistingDestIdPropertyValue = Guid.NewGuid().ToString();

        //    const string relationshipType = "relationshipType";
        //    const string relationshipVariable = "r";

        //    // create source node to create relationship from
        //    await MergeNode(sourceNodeLabel, sourceIdPropertyName,
        //        new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

        //    // create destination node for preexisting relationship
        //    await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, preexistingDestIdPropertyValue}});

        //    // create destination node to create new relationship to
        //    await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue}});

        //    // create pre-existing relationships
        //    var preexistingQuery = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };

        //    preexistingQuery.AddRelationshipsTo(relationshipType, new [] {destNodeLabel}, destIdPropertyName, destIdPropertyValue);

        //    await _graphDatabase.RunWriteQueries(preexistingQuery);

        //    // act
        //    var query = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };

        //    query.AddRelationshipsTo(relationshipType, new [] {destNodeLabel}, destIdPropertyName);

        //    await _graphDatabase.RunWriteQueries(query);

        //    AssertResult(relationshipVariable,new ExpectedRelationship[0],
        //        await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
        //        relationshipType, destNodeLabel, relationshipVariable));
        //}

        ////todo: complete test
        //[Fact(Skip="Need to improve AssertResult")]
        //public async Task ReplaceRelationships_CreateMultipleNewRelationship_NoExistingRelationships_Test()
        //{
        //    const string sourceNodeLabel = "sourceNodeLabel";
        //    const string sourceIdPropertyName = "sourceId";
        //    string sourceIdPropertyValue = Guid.NewGuid().ToString();

        //    const string destNodeLabel = "destNodeLabel";
        //    const string destIdPropertyName = "destId";
        //    string destIdPropertyValue1 = Guid.NewGuid().ToString();
        //    string destIdPropertyValue2 = Guid.NewGuid().ToString();

        //    const string relationshipType = "relationshipType";
        //    const string relationshipVariable = "r";

        //    //todo: arrange without any of the cut?
        //    // create source node to create relationship from
        //    long sourceNodeId = await MergeNode(sourceNodeLabel, sourceIdPropertyName,
        //        new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

        //    // create first destination node to create relationship to
        //    long destNodeId1 = await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue1}});

        //    // create second destination node to create relationship to
        //    long destNodeId2 = await MergeNode(destNodeLabel, destIdPropertyName,
        //        new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue2}});

        //    // act
        //    var query = new ReplaceRelationshipsCommand
        //    {
        //        SourceNodeLabels = new HashSet<string> {sourceNodeLabel},
        //        SourceIdPropertyName = sourceIdPropertyName,
        //        SourceIdPropertyValue = sourceIdPropertyValue
        //    };

        //    query.AddRelationshipsTo(relationshipType, new [] {destNodeLabel}, destIdPropertyName,
        //        destIdPropertyValue1, destIdPropertyValue2);

        //    await _graphDatabase.RunWriteQueries(query);

        //    AssertResult(relationshipVariable,new[]
        //    {
        //        new ExpectedRelationship
        //        {
        //            Type = relationshipType,
        //            StartNodeId = sourceNodeId,
        //            EndNodeId = destNodeId1,
        //            Properties = new Dictionary<string, object>()
        //        },
        //        new ExpectedRelationship
        //        {
        //            Type = relationshipType,
        //            StartNodeId = sourceNodeId,
        //            EndNodeId = destNodeId2,
        //            Properties = new Dictionary<string, object>()
        //        }
        //    }, await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
        //        relationshipType, destNodeLabel, relationshipVariable));
        //}

        //todo:
        //        public async Task ReplaceRelationships_CreateMultipleNewRelationship_ExistingRelationship_Test()
    }
}
