using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Extensions;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    // relationshiptocreate? as opposed to the driver's returned relationship
    public sealed class CommandRelationship : ICommandRelationship
    {
        public string RelationshipType { get; } // RelationshipType, not type to differentiate from System.Type
        public IDictionary<string, object>? Properties { get; }
        public IEnumerable<string> DestinationNodeLabels { get; }
        public string DestinationNodeIdPropertyName { get; }
        public IList<object> DestinationNodeIdPropertyValues { get; }

        public CommandRelationship(
            string relationshipType,
            IEnumerable<KeyValuePair<string, object>>? properties,
            IEnumerable<string> destinationNodeLabels,
            string destinationNodeIdPropertyName,
            IEnumerable<object>? destinationNodeIdPropertyValues)
        {
            RelationshipType = relationshipType;
            Properties = properties?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            DestinationNodeLabels = destinationNodeLabels;
            DestinationNodeIdPropertyName = destinationNodeIdPropertyName;
            DestinationNodeIdPropertyValues = new List<object>(destinationNodeIdPropertyValues ?? Enumerable.Empty<object>());
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

        private bool Equals(CommandRelationship other)
        {
            return RelationshipType == other.RelationshipType
                   && Equals(Properties, other.Properties)
                   && DestinationNodeLabels.Equals(other.DestinationNodeLabels)
                   && DestinationNodeIdPropertyName == other.DestinationNodeIdPropertyName
                   && DestinationNodeIdPropertyValues.Equals(other.DestinationNodeIdPropertyValues);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((CommandRelationship) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                RelationshipType,
                Properties,
                DestinationNodeLabels,
                DestinationNodeIdPropertyName,
                DestinationNodeIdPropertyValues);
        }

        public override string ToString()
        {
            return $@"[:{RelationshipType}]->(:{string.Join(":", DestinationNodeLabels)} {string.Join(",", Properties.ToCypherPropertiesString())})
{DestinationNodeIdPropertyName}:
{string.Join(Environment.NewLine, DestinationNodeIdPropertyValues)}";
        }
    }
}
