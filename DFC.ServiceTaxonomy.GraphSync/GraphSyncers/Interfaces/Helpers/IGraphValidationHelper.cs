﻿using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IGraphValidationHelper
    {
        (bool matched, string failureReason) ContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JValue, object, bool> areBothSame);

        (bool matched, string failureReason) ContentArrayPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JValue, object, bool> areBothSame);

        (bool matched, string failureReason) StringArrayContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        (bool matched, string failureReason) ContentMultilineStringPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        (bool matched, string failureReason) StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        public (bool matched, string failureReason) BoolContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        (bool matched, string failureReason) LongContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        public (bool matched, string failureReason) EnumContentPropertyMatchesNodeProperty<T>(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
            where T : Enum;

        (bool matched, string failureReason) DateTimeContentPropertyMatchesNodeProperty(
          string contentKey,
          JObject contentItemField,
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
