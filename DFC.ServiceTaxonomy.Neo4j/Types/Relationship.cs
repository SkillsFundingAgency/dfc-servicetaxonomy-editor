using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Extensions;

namespace DFC.ServiceTaxonomy.Neo4j.Types
{
    // relationshiptocreate? as opposed to the driver's returned relationship
    //todo: interface?
    public class Relationship
    {
        //source properties belong in here really, unless rename class

        public string RelationshipType { get; } // RelationshipType, not type to differentiate from System.Type
        public Dictionary<string, object>? Properties { get; }
        public IEnumerable<string> DestinationNodeLabels { get; }
        public string DestinationNodeIdPropertyName { get; }
        public IEnumerable<object> DestinationNodeIdPropertyValues { get; }

        public Relationship(
            string relationshipType,
            IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destinationNodeLabels,
            string destinationNodeIdPropertyName,
            IEnumerable<object> destinationNodeIdPropertyValues)
        {
            RelationshipType = relationshipType;
            Properties = properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            DestinationNodeLabels = destinationNodeLabels;
            DestinationNodeIdPropertyName = destinationNodeIdPropertyName;
            DestinationNodeIdPropertyValues = destinationNodeIdPropertyValues;
        }

        public List<string> ValidationErrors
        {
            get
            {
                List<string> errors = new List<string>();

                if (!DestinationNodeLabels.Any())
                    errors.Add($"Missing {nameof(DestinationNodeLabels)}.");

                return errors;
            }
        }

        public override string ToString()
        {
            return $@"[:{RelationshipType}]->(:{string.Join(":", DestinationNodeLabels)} {string.Join(",", Properties.ToCypherPropertiesString())})
{DestinationNodeIdPropertyName}:
{string.Join(Environment.NewLine, DestinationNodeIdPropertyValues)}";
        }
    }
}
