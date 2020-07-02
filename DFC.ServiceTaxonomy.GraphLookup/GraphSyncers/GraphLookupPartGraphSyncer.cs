using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.GraphSyncers
{
    public class GraphLookupPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(GraphLookupPart);

        private const string NodesPropertyName = "Nodes";

        public Task AddSyncComponents(JObject graphLookupContent,
            IGraphMergeContext context)
        {
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            JArray? nodes = (JArray?)graphLookupContent[NodesPropertyName];
            if (nodes == null || nodes.Count == 0)
                return Task.CompletedTask;

            if (settings.PropertyName != null)
            {
                mergeNodeCommand.Properties.Add(settings.PropertyName, GetId(nodes.First()));
            }

            if (settings.RelationshipType != null)
            {
                //todo: settings should contains destnodelabels
                replaceRelationshipsCommand.AddRelationshipsTo(
                    settings.RelationshipType!,
                    null,
                    new[] {settings.NodeLabel!},
                    settings.ValueFieldName!,
                    nodes.Select(GetId).ToArray());
            }

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            IValidateAndRepairGraph validateAndRepairGraph)
        {
            GraphLookupPart? graphLookupPart = content.ToObject<GraphLookupPart>();
            if (graphLookupPart == null)
                throw new GraphSyncException("Missing GraphLookupPart in content");

            GraphLookupPartSettings graphLookupPartSettings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            foreach (var node in graphLookupPart.Nodes)
            {
                string relationshipType = graphLookupPartSettings.RelationshipType!;

                (bool validated, string failureReason) = graphValidationHelper.ValidateOutgoingRelationship(
                    nodeWithOutgoingRelationships,
                    relationshipType,
                    graphLookupPartSettings.ValueFieldName!,
                    node.Id);

                if (!validated)
                    return Task.FromResult((false, failureReason));

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.TryGetValue(relationshipType, out int currentCount);
                expectedRelationshipCounts[relationshipType] = ++currentCount;
            }

            return Task.FromResult((true, ""));
        }

        private object GetId(JToken jToken)
        {
            return jToken["Id"]?.ToString() ??
                throw new GraphSyncException("Missing id in GraphLookupPart content.");
        }
    }
}
