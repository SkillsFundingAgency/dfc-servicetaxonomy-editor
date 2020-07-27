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

                //todo: rename variables to match delete?
                const string sourceNodeVariableName = "s";
                const string destinationNodeVariableBase = "d";
                const string newRelationshipVariableBase = "nr";
                const string destinationNodeOutgoingRelationshipsVariableBase = "dr";

                //todo: bi-directional relationships
                const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
                StringBuilder nodeMatchBuilder = new StringBuilder(
                        $"match ({sourceNodeVariableName}:{string.Join(':', _replaceRelationshipsCommand.SourceNodeLabels)} {{{_replaceRelationshipsCommand.SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
                StringBuilder destNodeOutgoingRelationshipsBuilder = new StringBuilder();
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
                        string destinationNodeOutgoingRelationshipsVariable = $"{destinationNodeOutgoingRelationshipsVariableBase}{ordinal}";

                        //todo: relationshiptype as parameter?
                        nodeMatchBuilder.Append(
                            $"\r\nmatch ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable}:{destNodeLabels} {{{relationship.DestinationNodeIdPropertyName}:${destIdPropertyValueParamName}}})");
                        parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                        if (_deleteDestinationNodes)
                        {
                            destNodeOutgoingRelationshipsBuilder.Append(
                                $"\r\noptional match ({destNodeVariable})-[{destinationNodeOutgoingRelationshipsVariable}]->()");
                        }
                    }
                }

                StringBuilder queryBuilder = new StringBuilder($"{nodeMatchBuilder}\r\n");

                if (_deleteDestinationNodes)
                {
                    // delete outgoing relationships on destination nodes first
                    queryBuilder.AppendLine(destNodeOutgoingRelationshipsBuilder.ToString());
                    queryBuilder.AppendLine(
                        $"delete {AllVariablesString(destinationNodeOutgoingRelationshipsVariableBase, ordinal)}");
                }

                // delete relationships from source node to destination nodes
                queryBuilder.AppendLine($"delete {AllVariablesString(newRelationshipVariableBase, ordinal)}");

                if (_deleteDestinationNodes)
                {
                    // then delete destination nodes
                    // note: any incoming relationships to the destination nodes (not from our source node)
                    // will stop this from executing
                    //todo: cancel publish/save if this fails (will e.g. stop taxonomy location terms being deleted if in use by pages)
                    queryBuilder.AppendLine($"delete {AllVariablesString(destinationNodeVariableBase, ordinal)}");
                }

                return new Query(queryBuilder.ToString(), parameters);
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
