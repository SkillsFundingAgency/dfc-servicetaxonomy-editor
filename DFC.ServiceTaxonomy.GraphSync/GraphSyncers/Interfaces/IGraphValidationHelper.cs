using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IGraphValidationHelper
    {
        (bool matched, string failureReason) ContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode,
            Func<JValue, object, bool> areBothSame);

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
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId,
            IReadOnlyDictionary<string, object>? properties = null);
    }
}
