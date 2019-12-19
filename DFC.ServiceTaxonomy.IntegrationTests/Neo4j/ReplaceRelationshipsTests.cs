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

            //todo: needs to return id
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
            Query query = QueryGenerator.ReplaceRelationships(sourceNodeLabel, sourceIdPropertyName, sourceIdPropertyValue, relationships);
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
    }
}
