using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IGraphValidationHelper
    {
        (bool matched, string failureReason) StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode);

        public (bool validated, string failureReason) ValidateOutgoingRelationship(
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            string relationshipType,
            string destinationIdPropertyName,
            object destinationId);
    }
}
