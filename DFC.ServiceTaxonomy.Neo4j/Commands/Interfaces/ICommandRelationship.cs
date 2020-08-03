using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface ICommandRelationship
    {
        string RelationshipType { get; } // RelationshipType, not type to differentiate from System.Type
        IDictionary<string, object>? Properties { get; }
        IEnumerable<string> DestinationNodeLabels { get; }
        string DestinationNodeIdPropertyName { get; }
        IList<object> DestinationNodeIdPropertyValues { get; }
    }
}
