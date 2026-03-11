using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IGraphValidationHelper
    {
        (bool matched, string failureReason) ContentPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JsonValue, object, bool> areBothSame);

        (bool matched, string failureReason) ContentArrayPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JsonValue, object, bool> areBothSame);

        (bool matched, string failureReason) StringArrayContentPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        (bool matched, string failureReason) ContentMultilineStringPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        (bool matched, string failureReason) StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        public (bool matched, string failureReason) BoolContentPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        (bool matched, string failureReason) LongContentPropertyMatchesNodeProperty(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        public (bool matched, string failureReason) EnumContentPropertyMatchesNodeProperty<T>(
            string contentKey,
            JsonObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
            where T : Enum;

        (bool matched, string failureReason) DateTimeContentPropertyMatchesNodeProperty(
          string contentKey,
          JsonObject contentItemField,
          string nodePropertyName,
          INode sourceNode);

        public (bool validated, string failureReason) ValidateOutgoingRelationship(
            ISubgraph nodeWithOutgoingRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId,
            IEnumerable<KeyValuePair<string, object>>? properties = null);

        (bool validated, string failureReason) ValidateIncomingRelationship(
            ISubgraph nodeWithIncomingRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId,
            IEnumerable<KeyValuePair<string, object>>? properties = null);
    }
}
