using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: now we delete all relationships, we should be able to just create rather than merge. change once we have integration test coverage
    //todo: refactor to only create relationships to one dest node label?

    public class ReplaceRelationshipsCommand : NodeWithOutgoingRelationshipsCommand, IReplaceRelationshipsCommand
    {
        public override Query Query
        {
            get
            {
                this.CheckIsValid();

                const string sourceNodeVariableName = "s";
                const string destinationNodeVariableBase = "d";
                const string newRelationshipVariableBase = "nr";
                const string newIncomingRelationshipVariableBase = "nir";
                const string relationshipPropertiesVariableBase = "rp";

                //todo: bi-directional relationships
                const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
                StringBuilder nodeMatchBuilder = new StringBuilder(
                        $"match ({sourceNodeVariableName}:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
                StringBuilder mergeBuilder = new StringBuilder();
                var parameters =
                    new Dictionary<string, object> {{sourceIdPropertyValueParamName, SourceIdPropertyValue!}};
                int ordinal = 0;
                //todo: better name relationship=> relationships, relationships=>?

                var distinctRelationshipTypeToDestNode = new HashSet<(string type, string labels)>();

                foreach (var relationship in RelationshipsList)
                {
                    string destNodeLabels = string.Join(':', relationship.DestinationNodeLabels.OrderBy(l => l));
                    // different types could have different dest node labels
                    // add unit/integration tests for this ^^ scenario
                    distinctRelationshipTypeToDestNode.Add((relationship.RelationshipType, destNodeLabels));

                    foreach (object destIdPropertyValue in relationship.DestinationNodeIdPropertyValues)
                    {
                        string relationshipVariable = $"{newRelationshipVariableBase}{++ordinal}";
                        string incomingRelationshipVariable = $"{newIncomingRelationshipVariableBase}{ordinal}";
                        string destNodeVariable = $"{destinationNodeVariableBase}{ordinal}";
                        string destIdPropertyValueParamName = $"{destNodeVariable}Value";

                        nodeMatchBuilder.Append(
                            $"\r\nmatch ({destNodeVariable}:{destNodeLabels} {{{relationship.DestinationNodeIdPropertyName}:${destIdPropertyValueParamName}}})");
                        parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                        mergeBuilder.Append(
                            $"\r\nmerge ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable})");

                        if (relationship.Properties?.Any() == true)
                        {
                            string relationshipPropertyName = $"{relationshipPropertiesVariableBase}{ordinal}";
                            mergeBuilder.Append($" set {relationshipVariable}=${relationshipPropertyName}");
                            parameters.Add(relationshipPropertyName, relationship.Properties);
                        }

                        //todo: deleting incoming relationships? don't own incoming relationships
                        // might have to look at old item (in publishing) and remove any existing we're removing

                        if (relationship.IncomingRelationshipType != null)
                        {
                            mergeBuilder.Append(
                                $"\r\nmerge ({sourceNodeVariableName})<-[{incomingRelationshipVariable}:{relationship.IncomingRelationshipType}]-({destNodeVariable})");
                        }

                        //todo: set properties also on incoming relationship?
                    }
                }

                string newRelationshipsVariablesString = AllVariablesString(newRelationshipVariableBase, ordinal);

                (string existingRelationshipsOptionalMatches, string existingRelationshipsVariablesString)
                    = GenerateExistingRelationships(distinctRelationshipTypeToDestNode, sourceNodeVariableName);

                string returnString =
                    mergeBuilder.Length > 0 ? $"return {newRelationshipsVariablesString}" : string.Empty;

                return new Query(
$@"{nodeMatchBuilder}
{existingRelationshipsOptionalMatches}
delete {existingRelationshipsVariablesString}
{mergeBuilder}
{returnString}",
                    parameters);
            }
        }

        private static (string optionalMatches, string variablesString) GenerateExistingRelationships(
            HashSet<(string type, string labels)> distinctRelationshipTypeToDestNode,
            string sourceNodeVariableName)
        {
            const string existingRelationshipVariableBase = "er";

            var existingRelationshipsMatchBuilder = new StringBuilder();

            int ordinal = 0;
            foreach ((string type, string labels) in distinctRelationshipTypeToDestNode)
            {
                existingRelationshipsMatchBuilder.Append(
                    $"\r\noptional match ({sourceNodeVariableName})-[{existingRelationshipVariableBase}{++ordinal}:{type}]->(:{labels})");
            }

            return (existingRelationshipsMatchBuilder.ToString(), AllVariablesString(existingRelationshipVariableBase, ordinal));
        }

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo: log query (doesn't work!) Query was: {resultSummary.Query}.
            //todo: validate deletes?
            // we should only query if the quick tests have failed, otherwise we'll slow down import a lot if we queried after every update

            int expectedOutgoingRelationshipsCreated = RelationshipsList.Sum(r => r.DestinationNodeIdPropertyValues.Count());

            // we don't know how many will be created if we're creating incoming relationships, as we don't delete them first,
            // so we don't check RelationshipsCreated if we have any
            if (RelationshipsList.All(r => r.IncomingRelationshipType == null)
                && resultSummary.Counters.RelationshipsCreated != expectedOutgoingRelationshipsCreated)
            {
                throw CreateValidationException(resultSummary,
                    $"Expected {expectedOutgoingRelationshipsCreated} relationships to be created, but {resultSummary.Counters.RelationshipsCreated} were created.");
            }

            //todo: can we do better?
            // int expectedIncomingRelationshipsCreated = RelationshipsList
            //     .Where(r => r.IncomingRelationshipType != null)
            //     .Sum(r => r.DestinationNodeIdPropertyValues.Count());
            // int expectedRelationshipsCreated =
            //     expectedOutgoingRelationshipsCreated + expectedIncomingRelationshipsCreated;

            if (!RelationshipsList.Any() || RelationshipsList.All(r => !r.DestinationNodeIdPropertyValues.Any()))
                return;

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
    }
}
