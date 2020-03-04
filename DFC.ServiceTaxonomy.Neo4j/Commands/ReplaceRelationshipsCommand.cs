using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Types;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: now we delete all relationships, we should be able to just create rather than merge. change once we have integration test coverage
    //todo: refactor to only create relationships to one dest node label?

    public class ReplaceRelationshipsCommand : IReplaceRelationshipsCommand
    {
        public HashSet<string> SourceNodeLabels { get; set; } = new HashSet<string>();
        public string? SourceIdPropertyName { get; set; }
        public string? SourceIdPropertyValue { get; set; }

        public IEnumerable<Relationship> Relationships
        {
            get { return RelationshipsList; }
        }

        private List<Relationship> RelationshipsList { get; set; } = new List<Relationship>();

        public void AddRelationshipsTo(string relationshipType, IEnumerable<string> destNodeLabels,
            string destIdPropertyName, params object[] destIdPropertyValues)
        {
            RelationshipsList.Add(new Relationship(relationshipType, null, destNodeLabels, destIdPropertyName,
                destIdPropertyValues));
        }

        public void CheckIsValid()
        {
            if (SourceNodeLabels == null)
                throw new InvalidOperationException($"{nameof(SourceNodeLabels)} is null");

            if (!SourceNodeLabels.Any())
                throw new InvalidOperationException("No source labels");

            if (SourceIdPropertyName == null)
                throw new InvalidOperationException($"{nameof(SourceIdPropertyName)} is null");

            if (SourceIdPropertyValue == null)
                throw new InvalidOperationException($"{nameof(SourceIdPropertyValue)} is null");
        }

        private Query CreateQuery()
        {
            CheckIsValid();

            const string sourceNodeVariableName = "s";
            const string destinationNodeVariableBase = "d";
            const string existingRelationshipVariableBase = "er";
            const string newRelationshipVariableBase = "nr";

            //todo: bi-directional relationships
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var nodeMatchBuilder =
                new StringBuilder(
                    $"match ({sourceNodeVariableName}:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var existingRelationshipsMatchBuilder = new StringBuilder();
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, SourceIdPropertyValue!}};
            int ordinal = 0;
            //todo: better name relationship=> relationships, relationships=>?

            var distinctRelationshipTypeToDestNode = new HashSet<(string type, string labels)>();

            foreach (var relationship in RelationshipsList)
            {
                string destNodeLabels = string.Join(':', relationship.DestinationNodeLabels.OrderBy(l => l));
                // different types could have different dest node labels
                // add unit/integration tests for this ^^ scenario
                distinctRelationshipTypeToDestNode.Add((relationship.RelationshipType, destNodeLabels));

                foreach (string destIdPropertyValue in relationship.DestinationNodeIdPropertyValues)
                {
                    string relationshipVariable = $"{newRelationshipVariableBase}{++ordinal}";
                    string destNodeVariable = $"{destinationNodeVariableBase}{ordinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";

                    nodeMatchBuilder.Append(
                        $"\r\nmatch ({destNodeVariable}:{destNodeLabels} {{{relationship.DestinationNodeIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                    mergeBuilder.Append($"\r\nmerge ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable})");
                }
            }

            string newRelationshipsVariablesString = AllVariablesString(newRelationshipVariableBase, ordinal);

            ordinal = 0;
            foreach ((string type, string labels) in distinctRelationshipTypeToDestNode)
            {
                existingRelationshipsMatchBuilder.Append(
                    $"\r\noptional match ({sourceNodeVariableName})-[{existingRelationshipVariableBase}{++ordinal}:{type}]->(:{labels})");
            }

            string existingRelationshipsVariablesString = AllVariablesString(existingRelationshipVariableBase, ordinal);

            return new Query(
$@"{nodeMatchBuilder}
{existingRelationshipsMatchBuilder}
delete {existingRelationshipsVariablesString}
{mergeBuilder}
return {newRelationshipsVariablesString}",
                parameters);
        }

        private static string AllVariablesString(string variableBase, int ordinal) =>
            string.Join(',', Enumerable.Range(1, ordinal).Select(o => $"{variableBase}{o}"));

        public Query Query => CreateQuery();

        public static implicit operator Query(ReplaceRelationshipsCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // nothing yet
            // validation can check return from query and/or counters are in range in result summary and/or notifications

            //todo: lof query (doesn't work!) Query was: {resultSummary.Query}.
            int expectedRelationshipsCreated = RelationshipsList.Sum(r => r.DestinationNodeIdPropertyValues.Count());
            if (resultSummary.Counters.RelationshipsCreated != expectedRelationshipsCreated)
                throw new CommandValidationException($"Expected {expectedRelationshipsCreated} relationships to be created, but {resultSummary.Counters.RelationshipsCreated} were created.");

            var createdRelationships = records?.FirstOrDefault()?.Values?.Values.Cast<IRelationship>();
            if (createdRelationships == null)
                throw new CommandValidationException("New relationships not returned.");

            // could store variable name to type dic and use that to check instead
            var expectedRelationshipTypes = RelationshipsList.Select(r => r.RelationshipType).OrderBy(t => t);

            var actualRelationshipTypes = createdRelationships.Select(r => r.Type).OrderBy(t => t);
            if (!Enumerable.SequenceEqual(expectedRelationshipTypes, actualRelationshipTypes))
                throw new CommandValidationException($"Relationship types creates ({string.Join(",", actualRelationshipTypes)}) does not match expected ({string.Join(",", expectedRelationshipTypes)})");

            var firstStartNodeId = createdRelationships.First().StartNodeId;
            if (!createdRelationships.Skip(1).All(r => r.StartNodeId == firstStartNodeId))
                throw new CommandValidationException("Not all created relationships have the same source node.");
        }
    }
}
