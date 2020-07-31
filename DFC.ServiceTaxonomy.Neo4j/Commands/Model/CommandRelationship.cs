using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Extensions;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Model
{
    // relationshiptocreate? as opposed to the driver's returned relationship
    public sealed class CommandRelationship : ICommandRelationship
    {
        public string RelationshipType { get; } // RelationshipType, not type to differentiate from System.Type
        public string? IncomingRelationshipType { get; }
        public IDictionary<string, object>? Properties { get; }
        public IEnumerable<string> DestinationNodeLabels { get; }
        public string DestinationNodeIdPropertyName { get; }
        public IList<object> DestinationNodeIdPropertyValues { get; }

        public CommandRelationship(
            string outgoingRelationshipType,
            string? incomingRelationshipType,
            IEnumerable<KeyValuePair<string, object>>? properties,
            IEnumerable<string> destinationNodeLabels,
            string destinationNodeIdPropertyName,
            IEnumerable<object>? destinationNodeIdPropertyValues)
        {
            RelationshipType = outgoingRelationshipType;
            IncomingRelationshipType = incomingRelationshipType;
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
                   && IncomingRelationshipType == other.IncomingRelationshipType
                   && DestinationNodeIdPropertyName == other.DestinationNodeIdPropertyName

                   && ((Properties == null && other.Properties == null)
                        || (Properties != null && other.Properties != null
                        && Properties.SequenceEqual(other.Properties)))    //todo: irrespective of order

                   && DestinationNodeLabels.SequenceEqual(other.DestinationNodeLabels)

                   && ((DestinationNodeIdPropertyValues == null && other.DestinationNodeIdPropertyValues == null)
                       || (DestinationNodeIdPropertyValues != null && other.DestinationNodeIdPropertyValues != null
                       && DestinationNodeIdPropertyValues.SequenceEqual(other.DestinationNodeIdPropertyValues)));    //todo: irrespective of order
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
                IncomingRelationshipType,
                Properties,
                DestinationNodeLabels,
                DestinationNodeIdPropertyName,
                DestinationNodeIdPropertyValues);
        }

        public override string ToString()
        {
            //todo: incoming relationship
            return $@"[:{RelationshipType}]->(:{string.Join(":", DestinationNodeLabels)} {string.Join(",", Properties.ToCypherPropertiesString())})
{DestinationNodeIdPropertyName}:
{string.Join(Environment.NewLine, DestinationNodeIdPropertyValues)}";
        }
    }
}
