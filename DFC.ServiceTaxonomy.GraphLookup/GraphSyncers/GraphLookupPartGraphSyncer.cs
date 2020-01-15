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
                //todo: what to do if id null?
                nodeProperties.Add(settings.PropertyName, nodes.First()["Id"]!.ToString());
            }

            if (settings.RelationshipType != null)
            {
                nodeRelationships.Add(
                    (destNodeLabel: settings.NodeLabel!, destIdPropertyName: settings.ValueFieldName!,
                        //todo: what to do if id null?
                        relationshipType: settings.RelationshipType!), nodes.Select(n => n["Id"]!.ToString()));
            }

            return Task.CompletedTask;
        }
    }
}
