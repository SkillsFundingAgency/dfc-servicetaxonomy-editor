using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IReplaceRelationshipsCommand
    {
        HashSet<string> SourceNodeLabels { get; set; }
        string? SourceIdPropertyName { get; set; }
        string? SourceIdPropertyValue { get; set; }

        IEnumerable<Relationship> Relationships { get; }

        void AddRelationshipsTo(string relationshipType, IEnumerable<string> destNodeLabels, string destIdPropertyName,
            params object[] destIdPropertyValues);

        Query Query { get; }
    }
}
