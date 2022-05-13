using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphLookup.GraphSyncers
{
    public class GraphLookupPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(GraphLookupPart);

        private const string NodesPropertyName = "Nodes";

        public override Task AddSyncComponents(JObject graphLookupContent, IGraphMergeContext context)
        {
            var settings = context.ContentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            JArray? nodes = (JArray?)graphLookupContent[NodesPropertyName];
            if (nodes == null || nodes.Count == 0)
                return Task.CompletedTask;

            if (settings.PropertyName != null)
            {
                context.MergeNodeCommand.Properties.Add(settings.PropertyName, GetId(nodes.First()));
            }

            if (settings.RelationshipType != null)
            {
                //todo: settings should contains destnodelabels
                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                    settings.RelationshipType!,
                    null,
                    new[] {settings.NodeLabel!},
                    settings.ValueFieldName!,
                    nodes.Select(GetId).ToArray());
            }

            return Task.CompletedTask;
        }

        public override Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            var settings = context.ContentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            if (settings.RelationshipType != null)
            {
                context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
                    settings.RelationshipType!,
                    new[] {settings.NodeLabel!});
            }

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            GraphLookupPart? graphLookupPart = content.ToObject<GraphLookupPart>();
            if (graphLookupPart == null)
                throw new GraphSyncException("Missing GraphLookupPart in content");

            GraphLookupPartSettings graphLookupPartSettings = context.ContentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            foreach (var node in graphLookupPart.Nodes)
            {
                string relationshipType = graphLookupPartSettings.RelationshipType!;

                (bool validated, string failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithRelationships,
                    relationshipType,
                    graphLookupPartSettings.ValueFieldName!,
                    node.Id);

                if (!validated)
                    return Task.FromResult((false, failureReason));

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.TryGetValue(relationshipType, out int currentCount);
                context.ExpectedRelationshipCounts[relationshipType] = ++currentCount;
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
