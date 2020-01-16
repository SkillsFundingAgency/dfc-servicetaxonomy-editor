using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.GraphSyncers
{
    public class GraphLookupPartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(GraphLookupPart);

        public Task AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            JArray nodes = (JArray)graphLookupContent.Nodes;
            if (nodes.Count == 0)
                return Task.CompletedTask;

            if (settings.PropertyName != null)
            {
                nodeProperties.Add(settings.PropertyName, GetId(nodes.First()));
            }

            if (settings.RelationshipType != null)
            {
                nodeRelationships.Add(
                    (destNodeLabel: settings.NodeLabel!, destIdPropertyName: settings.ValueFieldName!,
                        relationshipType: settings.RelationshipType!), nodes.Select(GetId));
            }

            return Task.CompletedTask;
        }

        private string GetId(JToken jToken)
        {
            string? id = jToken["Id"]?.ToString();
            if (id == null)    // todo: create a GraphSyncException
                throw new InvalidOperationException("Missing id in GraphLookupPart content.");

            return id;
        }
    }
}
