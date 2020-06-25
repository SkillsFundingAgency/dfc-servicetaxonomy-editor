using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Extensions;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class GraphValidationHelper : IGraphValidationHelper
    {
        // a bit smelly?
        // public (bool matched, string failureReason) ContentPropertyMatchesNodeProperty<T>(
        //     string contentKey,
        //     JObject contentItemField,
        //     string nodePropertyName,
        //     INode sourceNode)
        // {
        //     sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);
        //
        //     JValue? contentItemFieldValue = (JValue?)contentItemField?[contentKey];
        //     if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
        //     {
        //         bool bothNull = nodePropertyValue == null;
        //         return (bothNull, bothNull?"":"content property value was null, but node property value was not null");
        //     }
        //
        //     if (nodePropertyValue == null)
        //     {
        //         return (false, "node property value was null, but content property value was not null");
        //     }
        //
        //     T contentPropertyValue = contentItemFieldValue.As<T>();
        //
        //     bool bothSame;
        //     switch (contentPropertyValue)
        //     {
        //         case string s:
        //             bothSame = Equals(contentPropertyValue, nodePropertyValue);
        //             break;
        //         case DateTime d:
        //             var nodeZonedDateTime = nodePropertyValue.As<ZonedDateTime>();
        //
        //             //OC DateTime pickers don't support Milliseconds so ignoring conversion from Neo nanoseconds to OC millisecond
        //             var nodeAsDateTime = new DateTime(nodeZonedDateTime.Year, nodeZonedDateTime.Month, nodeZonedDateTime.Day, nodeZonedDateTime.Hour, nodeZonedDateTime.Minute, nodeZonedDateTime.Second);
        //
        //             bothSame = Equals(contentPropertyValue, nodeAsDateTime);
        //             break;
        //         default:
        //             throw new NotImplementedException("type is not supported");
        //     }
        //
        //     return (bothSame, bothSame?"":$"content property value was '{contentPropertyValue}', but node property value was '{(string)nodePropertyValue}'");
        // }

        public (bool matched, string failureReason) ContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JValue, object, bool> areBothSame)
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

            bool bothSame = areBothSame(contentItemFieldValue, nodePropertyValue);

            return (bothSame, bothSame?"":$"content property value was '{contentItemFieldValue.ToString(CultureInfo.CurrentCulture)}', but node property value was '{nodePropertyValue}'");
        }

        public (bool matched, string failureReason) ContentArrayPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JValue, object, bool> areBothSame)
        {
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JArray? contentItemFieldArray = (JArray?)contentItemField?[contentKey];
            if (contentItemFieldArray == null || contentItemFieldArray.Type == JTokenType.Null)
            {
                bool bothNull = nodePropertyValue == null;
                return (bothNull, bothNull?"":"content property array was null, but node property array was not null");
            }

            if (nodePropertyValue == null)
            {
                return (false, "node property array was null, but content property array was not null");
            }

            IEnumerable<object> nodePropertyValues = (IEnumerable<object>)nodePropertyValue;

            if (contentItemFieldArray.Count != nodePropertyValues.Count())
            {
                return (false, GenerateErrorMessage($"content property array has {contentItemFieldArray.Count} items, node property array has {nodePropertyValues.Count()} items"));
            }

            var areSame = contentItemFieldArray.Zip(nodePropertyValues, (cv, nv) => areBothSame((JValue)cv, nv));
            if (areSame.Any(same => !same))
                return (false, GenerateErrorMessage());

            return (true, "");

            string GenerateErrorMessage(string? prefix = null)
            {
                return $"{prefix}{(prefix!=null?": ":"")}content property array was '{string.Join(", ", contentItemFieldArray.Select(v => v.ToString()))}', but node property value was '{nodePropertyValue}'";
            }
        }

        public (bool matched, string failureReason) StringArrayContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentArrayPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => nodeValue is string && Equals((string)contentValue!, nodeValue));
        }

        public (bool matched, string failureReason) StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => nodeValue is string && Equals((string)contentValue!, nodeValue));
        }

        public (bool matched, string failureReason) BoolContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => nodeValue is bool && Equals((bool)contentValue, nodeValue));
        }

        public (bool matched, string failureReason) LongContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => nodeValue is long && Equals((long)contentValue, nodeValue));
        }

        public (bool matched, string failureReason) EnumContentPropertyMatchesNodeProperty<T>(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
            where T : Enum
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => Equals(((T)(object)(int)contentValue).ToString(), nodeValue.As<string>()));
        }

        public (bool matched, string failureReason) DateTimeContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) =>
                {
                    if (!(nodeValue is ZonedDateTime nodeZonedDateTime))
                        return false;

                    DateTime contentPropertyValue = (DateTime)contentValue;

                    //OC DateTime pickers don't support Milliseconds so ignoring conversion from Neo nanoseconds to OC millisecond
                    var nodeAsDateTime = new DateTime(nodeZonedDateTime.Year, nodeZonedDateTime.Month, nodeZonedDateTime.Day, nodeZonedDateTime.Hour, nodeZonedDateTime.Minute, nodeZonedDateTime.Second);

                    return Equals(contentPropertyValue, nodeAsDateTime);
                });
        }

        public (bool validated, string failureReason) ValidateOutgoingRelationship(
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId,
            IReadOnlyDictionary<string, object>? properties = null)
        {
            IOutgoingRelationship outgoingRelationship =
                nodeWithOutgoingRelationships.OutgoingRelationships.SingleOrDefault(or =>
                    or.Relationship.Type == relationshipType
                    && Equals(or.DestinationNode.Properties[destinationIdPropertyName], destinationId));

            if (outgoingRelationship == default)
                return (false, $"{RelationshipDescription(relationshipType, destinationIdPropertyName, destinationId)} not found");

            if (properties != null && !AreEqual(properties, outgoingRelationship.Relationship.Properties))
                return (false, $"{RelationshipDescription(relationshipType, destinationIdPropertyName, destinationId)} has incorrect properties. expecting {properties.ToCypherPropertiesString()}, found {outgoingRelationship.Relationship.Properties.ToCypherPropertiesString()}");

            return (true, "");
        }

        private string RelationshipDescription(string relationshipType, string destinationIdPropertyName, object destinationId)
        {
            return $"relationship of type ':{relationshipType}' to destination node with id '{destinationIdPropertyName}={destinationId}'";
        }

        private bool AreEqual<K, V>(IReadOnlyDictionary<K, V> first, IReadOnlyDictionary<K, V> second)
            where K : notnull
        {
            return first.Count == second.Count && !first.Except(second).Any();
        }
    }
}
