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

        /// <summary>
        /// One relationship will be created for each destIdPropertyValue.
        /// If no destIdPropertyValues are supplied, then no relationships will be created,
        /// but any relationships of relationshipType, from the source node to nodes with destNodeLabels will still be removed.
        /// </summary>
        void AddRelationshipsTo(string relationshipType, IEnumerable<string> destNodeLabels, string destIdPropertyName,
            params object[] destIdPropertyValues);
    }
}
