using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.GraphSyncers
{
    public class GraphLookupPartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(GraphLookupPart);

        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;

        public GraphLookupPartGraphSyncer(IGraphSyncPartIdProperty graphSyncPartIdProperty)
        {
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
        }

        public Task<IEnumerable<ICommand>> AddSyncComponents(
            dynamic graphLookupContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            var emptyResult = Task.FromResult(Enumerable.Empty<ICommand>());

            JArray nodes = (JArray)graphLookupContent.Nodes;
            if (nodes.Count == 0)
                return emptyResult;

            if (settings.PropertyName != null)
            {
                mergeNodeCommand.Properties[settings.PropertyName] = GetId(nodes.First());
            }

            if (settings.RelationshipType != null)
            {
                //todo: settings should contains destnodelabels
                replaceRelationshipsCommand.AddRelationshipsTo(
                    settings.RelationshipType!,
                    new[] {settings.NodeLabel!},
                    settings.ValueFieldName!,
                    nodes.Select(GetId).ToArray());
            }

            return emptyResult;
        }
        
        public Task<bool> VerifySyncComponent(ContentItem contentItem, ContentTypePartDefinition contentTypePartDefinition, INode sourceNode,
            IEnumerable<IRelationship> relationships, IEnumerable<INode> destNodes)
        {
            GraphLookupPart graphLookupPart = contentItem.Content.GraphLookupPart.ToObject<GraphLookupPart>();

            if (graphLookupPart != null)
            {
                var relationshipType =
                    (string)contentTypePartDefinition.Settings["GraphLookupPartSettings"]!["RelationshipType"]!;

                foreach (var node in graphLookupPart.Nodes)
                {
                    var destNode = destNodes.SingleOrDefault(x =>
                        (string)x.Properties[_graphSyncPartIdProperty.Name] == node.Id);

                    if (destNode == null)
                    {
                        return Task.FromResult(false);
                    }

                    var relationship = relationships.SingleOrDefault(x =>
                        x.Type == relationshipType && x.EndNodeId == destNode.Id);

                    if (relationship == null)
                    {
                        return Task.FromResult(false);
                    }
                }
            }

            return Task.FromResult(true);
        }

        private object GetId(JToken jToken)
        {
            return jToken["Id"]?.ToString() ??
                throw new GraphSyncException("Missing id in GraphLookupPart content.");
        }
    }
}
