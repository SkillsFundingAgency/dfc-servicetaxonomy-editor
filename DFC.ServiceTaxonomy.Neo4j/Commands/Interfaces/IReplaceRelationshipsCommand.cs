using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IReplaceRelationshipsCommand
    {
        Query Initialise(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue,
            IReadOnlyDictionary<(string destNodeLabel,string destIdPropertyName,string relationshipType),IEnumerable<string>> relationships);

        Query Query { get; }
    }
}
