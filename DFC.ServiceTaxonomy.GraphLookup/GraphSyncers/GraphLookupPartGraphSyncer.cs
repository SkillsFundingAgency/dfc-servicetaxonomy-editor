using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.GraphSyncers
{
    public class GraphLookupPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName
        {
            get { return "GraphLookupPart"; }
        }

        public void AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            JArray nodes = (JArray)graphLookupContent.Nodes;
            if (nodes.Count == 0)
                return;

            if (settings.PropertyName != null)
            {
                nodeProperties.Add(settings.PropertyName, nodes.First()["Id"].ToString());
            }

            if (settings.RelationshipType != null)
            {
                nodeRelationships.Add(
                    (destNodeLabel: settings.NodeLabel!, destIdPropertyName: settings.ValueFieldName!,
                        relationshipType: settings.RelationshipType!), nodes.Select(n => n["Id"].ToString()));
            }
        }
    }
}
