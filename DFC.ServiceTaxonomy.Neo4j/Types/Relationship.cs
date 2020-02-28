using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Types
{
    // relationshiptocreate? as opposed to the driver's returned relationship
    //todo: interface?
    public class Relationship
    {
        //source properties belong in here really, unless rename class

        public Relationship(string relationshipType, IDictionary<string, object>? properties,
            IEnumerable<string> destinationNodeLabels, string destinationNodeIdPropertyName,
            IEnumerable<object> destinationNodeIdPropertyValues)
        {
            RelationshipType = relationshipType;
            Properties = properties;
            DestinationNodeLabels = destinationNodeLabels;
            DestinationNodeIdPropertyName = destinationNodeIdPropertyName;
            DestinationNodeIdPropertyValues = destinationNodeIdPropertyValues;
        }

        public string RelationshipType { get; } // RelationshipType, not type to differentiate from System.Type
        public IDictionary<string, object>? Properties { get; }
        public IEnumerable<string> DestinationNodeLabels { get; }
        public string DestinationNodeIdPropertyName { get; }
        public IEnumerable<object> DestinationNodeIdPropertyValues { get; }
    }
}
