using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public interface INodeWithOutgoingRelationshipsCommand : ICommand
    {
        HashSet<string> SourceNodeLabels { get; set; }
        string? SourceIdPropertyName { get; set; }
        object? SourceIdPropertyValue { get; set; }

        IEnumerable<CommandRelationship> Relationships { get; }

        /// <summary>
        /// One relationship will be created for each destIdPropertyValue.
        /// If no destIdPropertyValues are supplied, then no relationships will be created,
        /// but any relationships of relationshipType, from the source node to nodes with destNodeLabels will still be removed.
        /// </summary>
        void AddRelationshipsTo(
            string relationshipType,
            IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues);

        void AddRelationshipsTo(IEnumerable<CommandRelationship> commandRelationship);
    }

    public abstract class NodeWithOutgoingRelationshipsCommand : INodeWithOutgoingRelationshipsCommand
    {
        public HashSet<string> SourceNodeLabels { get; set; } = new HashSet<string>();
        public string? SourceIdPropertyName { get; set; }
        public object? SourceIdPropertyValue { get; set; }

        public IEnumerable<CommandRelationship> Relationships
        {
            get { return RelationshipsList; }
        }

        protected List<CommandRelationship> RelationshipsList { get; set; } = new List<CommandRelationship>();

        public void AddRelationshipsTo(
            string relationshipType,
            IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues)
        {
            RelationshipsList.Add(new CommandRelationship(relationshipType, properties, destNodeLabels, destIdPropertyName,
                destIdPropertyValues));
        }

        public void AddRelationshipsTo(IEnumerable<CommandRelationship> commandRelationship)
        {
            RelationshipsList.AddRange(commandRelationship);
        }

        public virtual List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            if (!SourceNodeLabels.Any())
                validationErrors.Add($"Missing {nameof(SourceNodeLabels)}.");

            if (SourceIdPropertyName == null)
                validationErrors.Add($"{nameof(SourceIdPropertyName)} is null.");

            if (SourceIdPropertyValue == null)
                validationErrors.Add($"{nameof(SourceIdPropertyValue)} is null.");

            foreach (var relationship in RelationshipsList)
            {
                var relationshipValidationErrors = relationship.ValidationErrors;
                if (relationshipValidationErrors.Any())
                {
                    validationErrors.Add($"{relationship.RelationshipType??"<Null Type>"} relationship invalid ({string.Join(",", relationshipValidationErrors)})");
                }
            }

            return validationErrors;
        }

        public static implicit operator Query(NodeWithOutgoingRelationshipsCommand c) => c.Query;

        protected static string AllVariablesString(string variableBase, int ordinal) =>
            string.Join(',', Enumerable.Range(1, ordinal).Select(o => $"{variableBase}{o}"));

        public override string ToString()
        {
            return $@"(:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}: '{SourceIdPropertyValue}'}})
Relationships:
{string.Join(Environment.NewLine, RelationshipsList)}";
        }

        public abstract Query Query { get; }
        public abstract void ValidateResults(List<IRecord> records, IResultSummary resultSummary);
    }

    //todo: now we delete all relationships, we should be able to just create rather than merge. change once we have integration test coverage
    //todo: refactor to only create relationships to one dest node label?

    public class ReplaceRelationshipsCommand : NodeWithOutgoingRelationshipsCommand, IReplaceRelationshipsCommand
    {
        // public HashSet<string> SourceNodeLabels { get; set; } = new HashSet<string>();
        // public string? SourceIdPropertyName { get; set; }
        // public object? SourceIdPropertyValue { get; set; }
        //
        // public IEnumerable<CommandRelationship> Relationships
        // {
        //     get { return RelationshipsList; }
        // }
        //
        // private List<CommandRelationship> RelationshipsList { get; set; } = new List<CommandRelationship>();
        //
        // public void AddRelationshipsTo(
        //     string relationshipType,
        //     IReadOnlyDictionary<string, object>? properties,
        //     IEnumerable<string> destNodeLabels,
        //     string destIdPropertyName,
        //     params object[] destIdPropertyValues)
        // {
        //     RelationshipsList.Add(new CommandRelationship(relationshipType, properties, destNodeLabels, destIdPropertyName,
        //         destIdPropertyValues));
        // }
        //
        // public void AddRelationshipsTo(IEnumerable<CommandRelationship> commandRelationship)
        // {
        //     RelationshipsList.AddRange(commandRelationship);
        // }
        //
        // public List<string> ValidationErrors()
        // {
        //     List<string> validationErrors = new List<string>();
        //
        //     if (!SourceNodeLabels.Any())
        //         validationErrors.Add($"Missing {nameof(SourceNodeLabels)}.");
        //
        //     if (SourceIdPropertyName == null)
        //         validationErrors.Add($"{nameof(SourceIdPropertyName)} is null.");
        //
        //     if (SourceIdPropertyValue == null)
        //         validationErrors.Add($"{nameof(SourceIdPropertyValue)} is null.");
        //
        //     foreach (var relationship in RelationshipsList)
        //     {
        //         var relationshipValidationErrors = relationship.ValidationErrors;
        //         if (relationshipValidationErrors.Any())
        //         {
        //             validationErrors.Add($"{relationship.RelationshipType??"<Null Type>"} relationship invalid ({string.Join(",", relationshipValidationErrors)})");
        //         }
        //     }
        //
        //     return validationErrors;
        // }

        public override Query Query
        {
            get
            {
                this.CheckIsValid();

                const string sourceNodeVariableName = "s";
                const string destinationNodeVariableBase = "d";
                const string newRelationshipVariableBase = "nr";
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

        // public static implicit operator Query(ReplaceRelationshipsCommand c) => c.Query;

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

        // private static string AllVariablesString(string variableBase, int ordinal) =>
        //     string.Join(',', Enumerable.Range(1, ordinal).Select(o => $"{variableBase}{o}"));

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo: log query (doesn't work!) Query was: {resultSummary.Query}.
            //todo: validate deletes?
            // we should only query if the quick tests have failed, otherwise we'll slow down import a lot if we queried after every update

            int expectedRelationshipsCreated = RelationshipsList.Sum(r => r.DestinationNodeIdPropertyValues.Count());
            if (resultSummary.Counters.RelationshipsCreated != expectedRelationshipsCreated)
                throw CreateValidationException(resultSummary,
                    $"Expected {expectedRelationshipsCreated} relationships to be created, but {resultSummary.Counters.RelationshipsCreated} were created.");

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

        private CommandValidationException CreateValidationException(IResultSummary resultSummary, string message)
        {
            return new CommandValidationException(@$"{message}
{nameof(ReplaceRelationshipsCommand)}:
{this}

{resultSummary}");
        }

//         public override string ToString()
//         {
//             return $@"(:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}: '{SourceIdPropertyValue}'}})
// Relationships:
// {string.Join(Environment.NewLine, RelationshipsList)}";
//         }

        //todo: enum?
        // public ICommand GetDeleteRelationshipsCommand(bool deleteDestinationNodes)
        // {
        //     return new DeleteRelationshipsCommand(this, deleteDestinationNodes);
        // }
    }
}
