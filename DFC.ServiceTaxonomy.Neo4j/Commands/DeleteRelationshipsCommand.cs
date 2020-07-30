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

        public override Query Query
        {
            get
            {
                //todo: optional or not?
                //todo: lots of shared code
                this.CheckIsValid();

                //todo: rename variables to match delete
                const string sourceNodeVariableName = "s";
                const string destinationNodeVariableBase = "d";
                const string newRelationshipVariableBase = "nr";
                const string destinationNodeOutgoingRelationshipsVariableBase = "dr";
                const string destinationNodeIncomingTwoWayRelationshipsVariableBase = "it";

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

                    foreach (object destIdPropertyValue in relationship.DestinationNodeIdPropertyValues)
                    {
                        string relationshipVariable = $"{newRelationshipVariableBase}{++ordinal}";
                        string destNodeVariable = $"{destinationNodeVariableBase}{ordinal}";
                        string destIdPropertyValueParamName = $"{destNodeVariable}Value";
                        string destinationNodeOutgoingRelationshipsVariable = $"{destinationNodeOutgoingRelationshipsVariableBase}{ordinal}";
                        string destinationNodeIncomingTwoWayRelationshipsVariable = $"{destinationNodeIncomingTwoWayRelationshipsVariableBase}{ordinal}";

                        //todo: relationshiptype as parameter?
                        nodeMatchBuilder.Append(
                            $"\r\nmatch ({sourceNodeVariableName})-[{relationshipVariable}:{relationship.RelationshipType}]->({destNodeVariable}:{destNodeLabels} {{{relationship.DestinationNodeIdPropertyName}:${destIdPropertyValueParamName}}})");
                        parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                        if (DeleteDestinationNodes)
                        {
                            destNodeOutgoingRelationshipsBuilder.Append(
                                $"\r\noptional match ({destNodeVariable})-[{destinationNodeOutgoingRelationshipsVariable}]->()");

                            destNodeOutgoingRelationshipsBuilder.Append(
                                $"\r\noptional match ({destNodeVariable})<-[{destinationNodeIncomingTwoWayRelationshipsVariable}: {{twoWay: TRUE}}]-()");
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

        private int _expectedDeleted;

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo: check this
            if (DeleteDestinationNodes)
            {
                if (resultSummary.Counters.NodesDeleted != _expectedDeleted)
                    throw CreateValidationException(resultSummary,
                        $"Expected {_expectedDeleted} nodes to be deleted, but {resultSummary.Counters.NodesDeleted} were deleted.");

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
