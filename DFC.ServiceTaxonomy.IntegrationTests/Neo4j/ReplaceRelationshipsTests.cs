using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j
{
    [Collection("Graph Database Integration")]
    public class ReplaceRelationshipsTests : GraphDatabaseIntegrationTest
    {
        public ReplaceRelationshipsTests(GraphDatabaseCollectionFixture graphDatabaseCollectionFixture)
            : base(graphDatabaseCollectionFixture)
        {
        }

        [Fact]
        public async Task ReplaceRelationships_CreateSingleNewRelationship_NoExistingRelationships_Test()
        {
            const string sourceNodeLabel = "sourceNodeLabel";
            const string sourceIdPropertyName = "sourceId";
            string sourceIdPropertyValue = Guid.NewGuid().ToString();

            const string destNodeLabel = "destNodeLabel";
            const string destIdPropertyName = "destId";
            string destIdPropertyValue = Guid.NewGuid().ToString();

            const string relationshipType = "relationshipType";
            const string relationshipVariable = "r";

            //todo: arrange without any of the cut?
            // create source node to create relationship from
            long sourceNodeId = await MergeNode(sourceNodeLabel, sourceIdPropertyName,
                new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

            // create destination node to create relationship to
            long destNodeId = await MergeNode(destNodeLabel, destIdPropertyName,
                new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue}});

            //todo: is readonly enough, or should we clone? probably need to clone
            var relationships = new ReadOnlyDictionary<(string,string,string),IEnumerable<string>>(
                new Dictionary<(string,string,string),IEnumerable<string>> {{(destNodeLabel, destIdPropertyName, relationshipType), new[] {destIdPropertyValue}}});

            // act
            Query query = new ReplaceRelationshipsCommand
            {
                SourceNodeLabel = sourceNodeLabel,
                SourceIdPropertyName = sourceIdPropertyName,
                SourceIdPropertyValue = sourceIdPropertyValue,
                Relationships = relationships
            };
            await _graphDatabase.RunWriteQueries(query);

            AssertResult(relationshipVariable,new[]
            {
                new ExpectedRelationship
                {
                    Type = relationshipType,
                    StartNodeId = sourceNodeId,
                    EndNodeId = destNodeId,
                    Properties = new Dictionary<string, object>()
                }
            }, await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
                relationshipType, destNodeLabel, relationshipVariable));
        }

        [Fact]
        public async Task ReplaceRelationships_CreateSingleNewRelationship_ExistingRelationship_Test()
        {
            const string sourceNodeLabel = "sourceNodeLabel";
            const string sourceIdPropertyName = "sourceId";
            string sourceIdPropertyValue = Guid.NewGuid().ToString();

            const string destNodeLabel = "destNodeLabel";
            const string destIdPropertyName = "destId";
            string destIdPropertyValue = Guid.NewGuid().ToString();

            string preexistingDestIdPropertyValue = Guid.NewGuid().ToString();

            const string relationshipType = "relationshipType";
            const string relationshipVariable = "r";

            //todo: needs to return id
            // create source node to create relationship from
            long sourceNodeId = await MergeNode(sourceNodeLabel, sourceIdPropertyName,
                new Dictionary<string, object> {{sourceIdPropertyName, sourceIdPropertyValue}});

            // create destination node for preexisting relationship
            await MergeNode(destNodeLabel, destIdPropertyName,
                new Dictionary<string, object> {{destIdPropertyName, preexistingDestIdPropertyValue}});

            // create destination node to create new relationship to
            long destNodeId = await MergeNode(destNodeLabel, destIdPropertyName,
                new Dictionary<string, object> {{destIdPropertyName, destIdPropertyValue}});

            // create pre-existing relationships
            //todo: don't use cut for arrangement
            var preExistingRelationships = new ReadOnlyDictionary<(string,string,string),IEnumerable<string>>(
                new Dictionary<(string,string,string),IEnumerable<string>> {{(destNodeLabel, destIdPropertyName, relationshipType), new[] {destIdPropertyValue}}});

            Query preexistingQuery = new ReplaceRelationshipsCommand
            {
                SourceNodeLabel = sourceNodeLabel,
                SourceIdPropertyName = sourceIdPropertyName,
                SourceIdPropertyValue = sourceIdPropertyValue,
                Relationships = preExistingRelationships
            };
            await _graphDatabase.RunWriteQueries(preexistingQuery);

            //todo: is readonly enough, or should we clone? probably need to clone
            var relationships = new ReadOnlyDictionary<(string,string,string),IEnumerable<string>>(
                new Dictionary<(string,string,string),IEnumerable<string>> {{(destNodeLabel, destIdPropertyName, relationshipType), new[] {destIdPropertyValue}}});

            // act
            Query query = new ReplaceRelationshipsCommand
            {
                SourceNodeLabel = sourceNodeLabel,
                SourceIdPropertyName = sourceIdPropertyName,
                SourceIdPropertyValue = sourceIdPropertyValue,
                Relationships = relationships
            };
            await _graphDatabase.RunWriteQueries(query);

            AssertResult(relationshipVariable,new[]
            {
                new ExpectedRelationship
                {
                    Type = relationshipType,
                    StartNodeId = sourceNodeId,
                    EndNodeId = destNodeId,
                    Properties = new Dictionary<string, object>()
                }
            }, await AllRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue,
                relationshipType, destNodeLabel, relationshipVariable));
        }

        //todo:
        //        public async Task ReplaceRelationships_CreateNoNewRelationship_NoExistingRelationships_Test()
        //        public async Task ReplaceRelationships_CreateNoNewRelationship_ExistingRelationship_Test()

        //        public async Task ReplaceRelationships_CreateMultipleNewRelationship_NoExistingRelationships_Test()
        //        public async Task ReplaceRelationships_CreateMultipleNewRelationship_ExistingRelationship_Test()
    }
}
