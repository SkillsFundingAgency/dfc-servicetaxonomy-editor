using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Types;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IReplaceRelationshipsCommand : ICommand
    {
        HashSet<string> SourceNodeLabels { get; set; }
        string? SourceIdPropertyName { get; set; }
        string? SourceIdPropertyValue { get; set; }

        IEnumerable<Relationship> Relationships { get; }

        void AddRelationshipsTo(string relationshipType, IEnumerable<string> destNodeLabels, string destIdPropertyName,
            params object[] destIdPropertyValues);
    }
}
