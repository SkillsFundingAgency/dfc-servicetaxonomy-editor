using System;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class GraphValidationHelper : IGraphValidationHelper
    {
        //todo: better name
        public (bool matched, string failureReason) StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JValue? contentItemFieldValue = (JValue?)contentItemField?[contentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                bool bothNull = nodePropertyValue == null;
                return (bothNull, bothNull?"":"content property value was null, but node property value was not null");
            }

            if (nodePropertyValue == null)
            {
                return (false, "node property value was null, but content property value was not null");
            }

            string contentPropertyValue = contentItemFieldValue.As<string>();
            bool bothSame = contentPropertyValue == (string)nodePropertyValue;
            return (bothSame, bothSame?"":$"content property value was '{contentPropertyValue}', but node property value was '{(string)nodePropertyValue}'");
        }

        //todo: better name
        public (bool matched, string failureReason) DateTimeContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JValue? contentItemFieldValue = (JValue?)contentItemField?[contentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                bool bothNull = nodePropertyValue == null;
                return (bothNull, bothNull ? "" : "content property value was null, but node property value was not null");
            }

            if (nodePropertyValue == null)
            {
                return (false, "node property value was null, but content property value was not null");
            }

            DateTime contentPropertyValue = contentItemFieldValue.As<DateTime>();

            var nodeZonedDateTime = nodePropertyValue.As<ZonedDateTime>();

            //OC DateTime pickers don't support Milliseconds so ignoring conversion from Neo nanoseconds to OC millisecond
            var nodeAsDateTime = new DateTime(nodeZonedDateTime.Year, nodeZonedDateTime.Month, nodeZonedDateTime.Day, nodeZonedDateTime.Hour, nodeZonedDateTime.Minute, nodeZonedDateTime.Second);

            bool bothSame = contentPropertyValue == nodeAsDateTime;

            return (bothSame, bothSame ? "" : $"content property value was '{contentPropertyValue}', but node property value was '{(string)nodePropertyValue}'");
        }

        public (bool validated, string failureReason) ValidateOutgoingRelationship(
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId)
        {
            IOutgoingRelationship outgoingRelationship =
                nodeWithOutgoingRelationships.OutgoingRelationships.SingleOrDefault(or =>
                    or.Relationship.Type == relationshipType
                    && Equals(or.DestinationNode.Properties[destinationIdPropertyName], destinationId));

            return outgoingRelationship == null
                ? (false, $"relationship of type ':{relationshipType}' to destination node with id '{destinationIdPropertyName}={destinationId}' not found")
                : (true, "");
        }
    }
}
