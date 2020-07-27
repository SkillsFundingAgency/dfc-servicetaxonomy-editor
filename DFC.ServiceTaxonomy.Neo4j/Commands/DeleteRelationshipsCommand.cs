using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    // todo: abstract RelationshipCommand or have in builder??
    public class DeleteRelationshipsCommand : IDeleteRelationshipsCommand
    {
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly bool _deleteDestinationNodes;

        public DeleteRelationshipsCommand(
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            bool deleteDestinationNodes)
        {
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _deleteDestinationNodes = deleteDestinationNodes;
        }

        public List<string> ValidationErrors()
        {
            //todo: common code with IReplaceRelationshipsCommand
            // factor out : where? IRelationshipsFromNode??

            return new List<string>();
        }

        public Query Query
        {
            get
            {
                //todo: optional or not?
                //todo: lots of shared code
                this.CheckIsValid();

                const string sourceNodeVariableName = "s";
                const string destinationNodeVariableBase = "d";
                const string newRelationshipVariableBase = "nr";

                //todo: bi-directional relationships
                const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
                StringBuilder nodeMatchBuilder = new StringBuilder(
                        $"match ({sourceNodeVariableName}:{string.Join(':', _replaceRelationshipsCommand.SourceNodeLabels)} {{{_replaceRelationshipsCommand.SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
                // StringBuilder mergeBuilder = new StringBuilder();
                var parameters =
                    new Dictionary<string, object> {{sourceIdPropertyValueParamName, _replaceRelationshipsCommand.SourceIdPropertyValue!}};
                int ordinal = 0;
                //todo: better name relationship=> relationships, relationships=>?

                var distinctRelationshipTypeToDestNode = new HashSet<(string type, string labels)>();

                foreach (var relationship in _replaceRelationshipsCommand.Relationships)
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

                        //todo: relationshiptype as parameter?
                        nodeMatchBuilder.Append(
                            $"\r\nmatch ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable}:{destNodeLabels} {{{relationship.DestinationNodeIdPropertyName}:${destIdPropertyValueParamName}}})");
                        parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                        // mergeBuilder.Append(
                        //     $"\r\nmerge ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable})");

                        // mergeBuilder.Append($"\r\ndelete {relationshipVariable}");
                        // if (_deleteDestinationNodes)
                        //     mergeBuilder.Append($"\r\ndelete {destNodeVariable}");
                    }
                }

                string deleteRelationships = $"delete {AllVariablesString(newRelationshipVariableBase, ordinal)}";
                string destNodesVariablesString = _deleteDestinationNodes
                    ? $"delete {AllVariablesString(destinationNodeVariableBase, ordinal)}"
                    : "";

                // (string existingRelationshipsOptionalMatches, string existingRelationshipsVariablesString)
                //     = GenerateExistingRelationships(distinctRelationshipTypeToDestNode, sourceNodeVariableName);
                //
                // string returnString =
                //     mergeBuilder.Length > 0 ? $"return {newRelationshipsVariablesString}" : string.Empty;

                return new Query(
$@"{nodeMatchBuilder}
{deleteRelationships}
{destNodesVariablesString}",
                    parameters);
            }
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo:
        }

        private static string AllVariablesString(string variableBase, int ordinal) =>
            string.Join(',', Enumerable.Range(1, ordinal).Select(o => $"{variableBase}{o}"));
    }
}
