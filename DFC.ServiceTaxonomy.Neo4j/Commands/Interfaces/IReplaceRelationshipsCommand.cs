using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    // IRelationshipsCommandBuilder (or IRelationshipsFromNode), plus methods GetReplaceRelationshipsCommand, GetDeleteRelationshipsCommand(bool deleteDestinationNode)
    // commands would accept IEnumerable<IRelationshipsFromNode> (could also accept graphreplicaSet, but prob not)
    // could do same with INode with methods GetDeleteNodeCommand & GetMergeNodeCommand
    // rename ICommandRelationship to relationship
    // INodeType?? -> GetDeleteNodeTypeCommand
    public interface IReplaceRelationshipsCommand : INodeWithOutgoingRelationshipsCommand
    {
        // have 'alias' to make it obvious what the intention is
        void RemoveAnyRelationshipsTo(
            string relationshipType,
            IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName)
        {
            AddRelationshipsTo(relationshipType, properties, destNodeLabels, destIdPropertyName);
        }
    }
}
