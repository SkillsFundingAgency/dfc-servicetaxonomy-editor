using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class GraphValidationHelper : IGraphValidationHelper
    {
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
                bool bothNull = nodePropertyValue == null || (nodePropertyValue is JValue && ((JValue)nodePropertyValue).Type == JTokenType.Null);
                return (bothNull, bothNull ? string.Empty : $"content property value was null, but node property value was not null (gvh - {nodePropertyName} - {nodePropertyValue})");
            }

            if (nodePropertyValue == null)
            {
                return (false, "node property value was null, but content property value was not null");
            }

            bool bothSame = areBothSame(contentItemFieldValue, nodePropertyValue);
            return (bothSame, bothSame ? string.Empty : $"content property value was '{JValueToString(contentItemFieldValue)}', but node property value was '{nodePropertyValue}'");
        }

        private string JValueToString(JValue jvalue)
        {
            return jvalue.Type switch
            {
                JTokenType.Date => jvalue.ToString("u"),
                _ => jvalue.ToString(CultureInfo.InvariantCulture)
            };
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
                return (bothNull, bothNull ? string.Empty : "content property array was null, but node property array was not null");
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

            return (true, string.Empty);

            string GenerateErrorMessage(string? prefix = null)
            {
                return $"{prefix}{(prefix!=null ? ": " : string.Empty)}content property array was '{string.Join(", ", contentItemFieldArray)}', but node property value was '{string.Join(", ", (IEnumerable<object>)nodePropertyValue)}'";
            }
        }

        public (bool matched, string failureReason) ContentMultilineStringPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);
            IEnumerable<string>? nodeStrings = (nodePropertyValue as IEnumerable<object>)?
                                                    .Select(no => no.ToString() ?? string.Empty)
                                                    .Where(ns => !string.IsNullOrEmpty(ns));

            if (nodeStrings == null)
                return (false, "expecting node property array of string");

            JToken? contentMultilineString = contentItemField?[contentKey];
            if (contentMultilineString == null || contentMultilineString.Type == JTokenType.Null)
            {
                bool bothEmpty = !nodeStrings.Any();
                return (bothEmpty, bothEmpty ? string.Empty : "content multiline string was null, but node property array had value(s)");
            }

            string[] contentStrings = contentMultilineString.Value<string>()?.Split("\r\n") ?? new string[0];

            if (contentStrings.Count() != nodeStrings.Count())
            {
                return (false, GenerateErrorMessage($"content multiline string had {contentStrings.Count()} lines, but node property array has {nodeStrings.Count()} items"));
            }

            var areSame = contentStrings.Zip(nodeStrings, string.Equals);
            if (areSame.Any(same => !same))
                return (false, GenerateErrorMessage());

            return (true, string.Empty);

            string GenerateErrorMessage(string? prefix = null)
            {
                return $"{prefix}{(prefix!=null?": " : string.Empty)}content multiline string was '{string.Join(", ", contentStrings)}', but node property value was '{string.Join(", ", nodeStrings)}'";
            }
        }

        public (bool matched, string failureReason) StringArrayContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentArrayPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => EqualsCheck<string>(contentValue, nodeValue, JTokenType.String));
        }

        public (bool matched, string failureReason) StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => StringEqualsCheck(contentValue, nodeValue));
        }

        public (bool matched, string failureReason) BoolContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => EqualsCheck<bool>(contentValue, nodeValue, JTokenType.Boolean));
        }

        public (bool matched, string failureReason) LongContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => EqualsCheck<long>(contentValue, nodeValue, JTokenType.Integer));
        }

        public (bool matched, string failureReason) EnumContentPropertyMatchesNodeProperty<T>(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
            where T : Enum
        {
            return ContentPropertyMatchesNodeProperty(contentKey, contentItemField, nodePropertyName, sourceNode,
                (contentValue, nodeValue) => Equals(((T)(object)(int)contentValue).ToString().ToLowerInvariant(), nodeValue.As<string>()));
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
                    var nodeAsDateTime = new DateTime(
                            nodeZonedDateTime.Year,
                            nodeZonedDateTime.Month,
                            nodeZonedDateTime.Day,
                            nodeZonedDateTime.Hour,
                            nodeZonedDateTime.Minute,
                            nodeZonedDateTime.Second)
                        .ToUniversalTime();

                    return Equals(contentPropertyValue, nodeAsDateTime);
                });
        }

        public (bool validated, string failureReason) ValidateOutgoingRelationship(
            ISubgraph nodeWithRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId,
            IEnumerable<KeyValuePair<string, object>>? properties = null)
        {
            IRelationship? outgoingRelationship =
                nodeWithRelationships.OutgoingRelationships
                    .SingleOrDefault(r =>
                     r.Type == relationshipType
                     && destinationId.ToString()!.EndsWith(
                         nodeWithRelationships.Nodes.First(n => n.Id == r.EndNodeId)
                             .Properties[destinationIdPropertyName].ToString()!));

            if (outgoingRelationship == null)
                return (false, $"{RelationshipDescription(relationshipType, destinationIdPropertyName, destinationId)} not found");

            if (properties != null && !AreEqual(properties, outgoingRelationship.Properties.Where(pr => pr.Key != "contentType")))
                return (false, $"{RelationshipDescription(relationshipType, destinationIdPropertyName, destinationId)} has incorrect properties. expecting {properties.ToCypherPropertiesString()}, found {outgoingRelationship.Properties.ToCypherPropertiesString()}");

            return (true, string.Empty);
        }

        public (bool validated, string failureReason) ValidateIncomingRelationship(
            ISubgraph nodeWithRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId,
            IEnumerable<KeyValuePair<string, object>>? properties = null)
        {
            IRelationship? incomingRelationship =
                nodeWithRelationships.IncomingRelationships.SingleOrDefault(r =>
                    r.Type.Equals(relationshipType, StringComparison.InvariantCultureIgnoreCase)
                    && destinationId.ToString()!.EndsWith(nodeWithRelationships.Nodes.First(n => n.Id == r.StartNodeId)
                        .Properties[destinationIdPropertyName].ToString()!, StringComparison.InvariantCultureIgnoreCase));

            if (incomingRelationship == null)
                return (false, $"{IncomingRelationshipDescription(relationshipType, destinationIdPropertyName, destinationId)} not found");

            if (properties != null && !AreEqual(properties, incomingRelationship.Properties.Where(pr => pr.Key != "contentType")))
                return (false, $"{IncomingRelationshipDescription(relationshipType, destinationIdPropertyName, destinationId)} has incorrect properties. expecting {properties.ToCypherPropertiesString()}, found {incomingRelationship.Properties.ToCypherPropertiesString()}");

            return (true, string.Empty);
        }

        private string RelationshipDescription(string relationshipType, string destinationIdPropertyName, object destinationId)
        {
            return $"relationship of type ':{relationshipType}' to destination node with id '{destinationIdPropertyName}={destinationId}'";
        }

        private string IncomingRelationshipDescription(string relationshipType, string destinationIdPropertyName, object destinationId)
        {
            return $"incoming relationship of type ':{relationshipType}' from node with id '{destinationIdPropertyName}={destinationId}'";
        }

        private bool AreEqual<K, V>(IEnumerable<KeyValuePair<K, V>> first, IEnumerable<KeyValuePair<K, V>> second)
            where K : notnull
        {
            return first.Count() == second.Count() && !first.Except(second).Any();
        }

        private bool StringEqualsCheck(JValue contentValue, object nodeValue)
        {
            var leftValue = contentValue.ToString();
            string rightValue;

            if (nodeValue is JValue jRightValue)
            {
                rightValue = jRightValue.ToString();
            }
            else
            {
                if (!(nodeValue is string castedNodeValue))
                {
                    return false;
                }

                rightValue = castedNodeValue;
            }

            SanitiseDecimalStringsForCompare(ref leftValue, ref rightValue);

            return Equals(leftValue, rightValue);
        }

        private bool EqualsCheck<T>(JValue contentValue, object nodeValue, JTokenType type)
        {
            T leftValue = contentValue.ToObject<T>()!;
            T rightValue;

            if (nodeValue is JValue jRightValue && jRightValue.Type == type)
            {
                rightValue = jRightValue.ToObject<T>()!;
            }
            else
            {
                if (!(nodeValue is T castedNodeValue))
                {
                    return false;
                }

                rightValue = castedNodeValue;
            }

            return Equals(leftValue, rightValue);
        }

        private void SanitiseDecimalStringsForCompare(ref string leftValue, ref string rightValue)
        {
            if(decimal.TryParse(leftValue, out var result))
            {
                leftValue = leftValue.TrimEnd('0');
                rightValue = rightValue.TrimEnd('0');
                leftValue = leftValue.EndsWith('.') ? leftValue.TrimEnd('.') : leftValue;
                rightValue = rightValue.EndsWith('.') ? rightValue.TrimEnd('.') : rightValue;
            }
        }
    }
}
