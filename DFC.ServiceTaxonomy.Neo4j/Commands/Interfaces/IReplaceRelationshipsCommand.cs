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
        bool ReplaceExistingRelationships { get; set; }

        //todo: this comment applies only to IReplaceRelationshipsCommand, but is declared in INodeWithOutgoingRelationshipsCommand

        /// <summary>
        /// One relationship will be created for each destIdPropertyValue.
        /// If no destIdPropertyValues are supplied, then no relationships will be created,
        /// but any relationships of relationshipType, from the source node to nodes with destNodeLabels will still be removed.
        /// </summary>
        // new void AddRelationshipsTo(
        //     string relationshipType,
        //     IReadOnlyDictionary<string, object>? properties,
        //     IEnumerable<string> destNodeLabels,
        //     string destIdPropertyName,
        //     params object[] destIdPropertyValues);

        //todo: shouldn't require destIdPropertyName (& properties if not matching on)
        // destIdPropertyName isn't actually used if no destIdPropertyIds
        // neither is properties

        // have 'alias' to make it obvious what the intention is
        void RemoveAnyRelationshipsTo(
            string relationshipType,
            //IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destNodeLabels)
            //string destIdPropertyName)
        {
            AddRelationshipsTo(relationshipType, null, destNodeLabels, null);
        }
    }
}
