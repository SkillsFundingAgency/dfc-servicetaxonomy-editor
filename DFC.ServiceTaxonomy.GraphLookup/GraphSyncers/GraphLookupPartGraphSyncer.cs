using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.GraphSyncers
{
    public class GraphLookupPartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(GraphLookupPart);

        public Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            //IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            JArray nodes = (JArray)graphLookupContent.Nodes;
            if (nodes.Count == 0)
                return Task.FromResult(Enumerable.Empty<Query>());

            if (settings.PropertyName != null)
            {
                nodeProperties.Add(settings.PropertyName, GetId(nodes.First()));
            }

            if (settings.RelationshipType != null)
            {
                //todo: settings should contains destnodelabels
                // nodeRelationships.Add(
                //     (destNodeLabel: settings.NodeLabel!, destIdPropertyName: settings.ValueFieldName!,
                //         relationshipType: settings.RelationshipType!), nodes.Select(GetId));
                replaceRelationshipsCommand.AddRelationshipsTo(
                    settings.RelationshipType!,
                    new[] {settings.NodeLabel!},
                    settings.ValueFieldName!,
                    nodes.Select(GetId).ToArray());
            }

            return Task.FromResult(Enumerable.Empty<Query>());
        }

        private string GetId(JToken jToken)
        {
            string? id = jToken["Id"]?.ToString();
            if (id == null)
                throw new GraphSyncException("Missing id in GraphLookupPart content.");

            return id;
        }
    }
}
