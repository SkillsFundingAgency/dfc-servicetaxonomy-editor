using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    // todo: abstract RelationshipCommand or have in builder??
    public class DeleteRelationshipsCommand : NodeWithOutgoingRelationshipsCommand, IDeleteRelationshipsCommand
    {
        public bool DeleteDestinationNodes { get; set; }

        private int _expectedDeleted;

        //todo: rename variables to match delete
        private const string sourceNodeVariableName = "s";
        private const string destinationNodeVariableBase = "d";
        private const string newRelationshipVariableBase = "nr";
        private const string destinationNodeOutgoingRelationshipsVariableBase = "dr";
        private const string destinationNodeIncomingTwoWayRelationshipsVariableBase = "it";

        public override Query Query
        {
            get
            {
                //todo: optional or not?
                //todo: lots of shared code
                this.CheckIsValid();

                //todo: bi-directional relationships
                const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
                StringBuilder nodeMatchBuilder = new StringBuilder(
                        $"match ({sourceNodeVariableName}:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
                StringBuilder destNodeOutgoingRelationshipsBuilder = new StringBuilder();
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

                    if (relationship.DestinationNodeIdPropertyName == null)
                    {
                        BuildForRelationship(++ordinal, nodeMatchBuilder, destNodeOutgoingRelationshipsBuilder,
                            parameters, relationship, destNodeLabels);
                    }
                    else
                    {
                        foreach (object destIdPropertyValue in relationship.DestinationNodeIdPropertyValues)
                        {
                            BuildForRelationship(++ordinal, nodeMatchBuilder, destNodeOutgoingRelationshipsBuilder,
                                parameters, relationship, destNodeLabels, destIdPropertyValue);
                            // string relationshipVariable = $"{newRelationshipVariableBase}{++ordinal}";
                            // string destNodeVariable = $"{destinationNodeVariableBase}{ordinal}";
                            // string destIdPropertyValueParamName = $"{destNodeVariable}Value";
                            // string destinationNodeOutgoingRelationshipsVariable = $"{destinationNodeOutgoingRelationshipsVariableBase}{ordinal}";
                            // string destinationNodeIncomingTwoWayRelationshipsVariable = $"{destinationNodeIncomingTwoWayRelationshipsVariableBase}{ordinal}";
                            //
                            // //todo: relationshiptype as parameter?
                            // //todo: use AppendLine instead?
                            // nodeMatchBuilder.Append(
                            //     $"\r\nmatch ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable}:{destNodeLabels})");
                            // nodeMatchBuilder.Append(
                            //     $"\r\nwhere {destNodeVariable}.{relationship.DestinationNodeIdPropertyName} = ${destIdPropertyValueParamName}");
                            // parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);
                            //
                            // if (!DeleteDestinationNodes)
                            //     continue;
                            //
                            // destNodeOutgoingRelationshipsBuilder.Append(
                            //     $"\r\noptional match ({destNodeVariable})-[{destinationNodeOutgoingRelationshipsVariable}]->()");
                            //
                            // destNodeOutgoingRelationshipsBuilder.Append(
                            //     $"\r\noptional match ({destNodeVariable})<-[{destinationNodeIncomingTwoWayRelationshipsVariable} {{{TwoWayRelationshipPropertyName}: TRUE}}]-()");
                        }
                    }
                }

                StringBuilder queryBuilder = new StringBuilder($"{nodeMatchBuilder}\r\n");

                if (DeleteDestinationNodes)
                {
                    // delete outgoing relationships on destination nodes first
                    queryBuilder.AppendLine(destNodeOutgoingRelationshipsBuilder.ToString());

                    queryBuilder.AppendLine(
                        $"delete {AllVariablesString(destinationNodeIncomingTwoWayRelationshipsVariableBase, ordinal)}");

                    queryBuilder.AppendLine(
                        $"delete {AllVariablesString(destinationNodeOutgoingRelationshipsVariableBase, ordinal)}");
                }

                // delete relationships from source node to destination nodes
                queryBuilder.AppendLine($"delete {AllVariablesString(newRelationshipVariableBase, ordinal)}");

                if (DeleteDestinationNodes)
                {
                    // then delete destination nodes
                    // note: any incoming relationships to the destination nodes (not from our source node)
                    // will stop this from executing
                    //todo: cancel publish/save if this fails (will e.g. stop taxonomy location terms being deleted if in use by pages)
                    queryBuilder.AppendLine($"delete {AllVariablesString(destinationNodeVariableBase, ordinal)}");
                }

                // don't use _expectedDeleted instead of ordinal, as could be changed by other threads calling Query
                // we should probably make class immutable, or stop mutations after Query has been called
                _expectedDeleted = ordinal;

                return new Query(queryBuilder.ToString(), parameters);
            }
        }

        private void BuildForRelationship(
            int ordinal,
            StringBuilder nodeMatchBuilder,
            StringBuilder destNodeOutgoingRelationshipsBuilder,
            Dictionary<string, object> parameters,
            ICommandRelationship relationship,
            // string relationshipType,
            string destNodeLabels,
            // string? destinationNodeIdPropertyName = null,
            object? destIdPropertyValue = null)
        {
            string relationshipVariable = $"{newRelationshipVariableBase}{ordinal}";
            string destNodeVariable = $"{destinationNodeVariableBase}{ordinal}";
            string destIdPropertyValueParamName = $"{destNodeVariable}Value";
            string destinationNodeOutgoingRelationshipsVariable = $"{destinationNodeOutgoingRelationshipsVariableBase}{ordinal}";
            string destinationNodeIncomingTwoWayRelationshipsVariable = $"{destinationNodeIncomingTwoWayRelationshipsVariableBase}{ordinal}";

            //todo: use AppendLine instead?
            nodeMatchBuilder.Append(
                $"\r\nmatch ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable}:{destNodeLabels})");
            if (relationship.DestinationNodeIdPropertyName != null)
            {
                nodeMatchBuilder.Append(
                    $"\r\nwhere {destNodeVariable}.{relationship.DestinationNodeIdPropertyName} = ${destIdPropertyValueParamName}");
                parameters.Add(destIdPropertyValueParamName, destIdPropertyValue!);
            }

            if (!DeleteDestinationNodes)
                return;

            destNodeOutgoingRelationshipsBuilder.Append(
                $"\r\noptional match ({destNodeVariable})-[{destinationNodeOutgoingRelationshipsVariable}]->()");

            destNodeOutgoingRelationshipsBuilder.Append(
                $"\r\noptional match ({destNodeVariable})<-[{destinationNodeIncomingTwoWayRelationshipsVariable} {{{TwoWayRelationshipPropertyName}: TRUE}}]-()");
        }

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            if (DeleteDestinationNodes)
            {
                if (resultSummary.Counters.NodesDeleted > _expectedDeleted)
                    throw CreateValidationException(resultSummary,
                        $"Expected no more than {_expectedDeleted} nodes to be deleted, but {resultSummary.Counters.NodesDeleted} were deleted.");

                // we don't know (without querying) how many relationships are deleted, if DeleteDestinationNodes is true
                // (due to not knowing how many outgoing relationships are on the destination nodes)
            }
            else
            {
                if (resultSummary.Counters.RelationshipsDeleted != _expectedDeleted)
                    throw CreateValidationException(resultSummary,
                        $"Expected {_expectedDeleted} relationships to be deleted, but {resultSummary.Counters.RelationshipsDeleted} were deleted.");
            }
        }
    }
}
