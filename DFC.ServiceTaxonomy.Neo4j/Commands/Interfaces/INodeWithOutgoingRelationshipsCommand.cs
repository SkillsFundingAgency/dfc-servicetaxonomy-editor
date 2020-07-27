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

        void AddRelationshipsTo(
            string relationshipType,
            IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues);

        void AddRelationshipsTo(IEnumerable<CommandRelationship> commandRelationship);
    }

}
