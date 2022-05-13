using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands
{
    //todo: now we delete all relationships, we should be able to just create rather than merge. change once we have integration test coverage
    //todo: rename now don't necessarily replace existing relationships

    public class CosmosDbReplaceRelationshipsCommand : CosmosDbNodeWithOutgoingRelationshipsCommand, IReplaceRelationshipsCommand
    {
        public bool ReplaceExistingRelationships { get; set; } = true;

        public override Query Query
        {
            get
            {
                this.CheckIsValid();

                var parameters = new Dictionary<string, object>
                {
                    { "uri", SourceIdPropertyValue! },
                    { "relationships", RelationshipsList },
                };

                return new Query("ReplaceRelationshipsCommand", parameters);
            }
        }

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            int expectedOutgoingRelationshipsCreated = RelationshipsList
                .Sum(r => r.IncomingRelationshipType == null
                    ? r.DestinationNodeIdPropertyValues.Count
                    : r.DestinationNodeIdPropertyValues.Count * 2);

            if (ReplaceExistingRelationships
                && RelationshipsList.All(r => r.IncomingRelationshipType == null))
            {
                if (resultSummary.Counters.RelationshipsCreated != expectedOutgoingRelationshipsCreated)
                {
                    throw CreateValidationException(resultSummary,
                        $"Expected {expectedOutgoingRelationshipsCreated} relationships to be created, but {resultSummary.Counters.RelationshipsCreated} were created.");
                }

                if (!RelationshipsList.Any() || RelationshipsList.All(r => !r.DestinationNodeIdPropertyValues.Any()))
                    return;

                //todo: we might be able to do some similar checks in the else clause below

                IEnumerable<IRelationship>? createdRelationships = records?.FirstOrDefault()?.Values?.Values.Cast<IRelationship>();
                if (createdRelationships == null)
                    throw CreateValidationException(resultSummary, "New relationships not returned.");

                // could store variable name to type dic and use that to check instead
                List<string> unorderedExpectedRelationshipTypes = RelationshipsList.SelectMany(
                    relationship => relationship.DestinationNodeIdPropertyValues,
                    (relationship, _) => relationship.RelationshipType).ToList();

                var expectedRelationshipTypes = unorderedExpectedRelationshipTypes
                    .OrderBy(t => t);

                var actualRelationshipTypes = createdRelationships
                    .Select(r => r.Type)
                    .OrderBy(t => t);

                if (!expectedRelationshipTypes.SequenceEqual(actualRelationshipTypes))
                    throw CreateValidationException(resultSummary,
                        $"Relationship types created ({string.Join(",", actualRelationshipTypes)}) does not match expected ({string.Join(",", expectedRelationshipTypes)})");

                var firstStartNodeId = createdRelationships.First().StartNodeId;
                if (!createdRelationships.Skip(1).All(r => r.StartNodeId == firstStartNodeId))
                    throw CreateValidationException(resultSummary,
                        "Not all created relationships have the same source node.");
            }
            else
            {
                // we don't know how many will be created if we're:
                // * creating incoming relationships
                // * not replacing existing relationships
                // as the number created depends on the initial graph state,
                // so instead we check that we haven't created more than expected
                if (resultSummary.Counters.RelationshipsCreated > expectedOutgoingRelationshipsCreated)
                {
                    throw CreateValidationException(resultSummary,
                        $"Expected no more than {expectedOutgoingRelationshipsCreated} relationships to be created, but {resultSummary.Counters.RelationshipsCreated} were created.");
                }
            }
        }
    }
}
