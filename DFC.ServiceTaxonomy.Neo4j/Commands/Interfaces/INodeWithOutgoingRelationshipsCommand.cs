using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface INodeWithOutgoingRelationshipsCommand : ICommand
    {
        HashSet<string> SourceNodeLabels { get; set; }
        string? SourceIdPropertyName { get; set; }
        object? SourceIdPropertyValue { get; set; }

        IEnumerable<CommandRelationship> Relationships { get; }

        void AddRelationshipsTo(string relationshipType,
            IEnumerable<KeyValuePair<string, object>>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues);

        void AddRelationshipsTo(IEnumerable<CommandRelationship> commandRelationship);

        //todo: replace existing with this? if incoming is null, then just creates outgoing
        public void AddTwoWayRelationships(string outgoingRelationshipType,
            string? incomingRelationshipType,
            IEnumerable<KeyValuePair<string, object>>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues);
    }
}
