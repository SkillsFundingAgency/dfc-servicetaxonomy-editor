using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.GraphSyncers
{
    public class GraphLookupPartSyncToGraph : ISyncPartToGraph //<GraphLookupPartSettings>
//    public class GraphLookupPartSyncToGraph : ISyncPartToGraph<GraphLookupPartSettings>
    {
        public string PartName
        {
            get { return "GraphLookupPart"; }
        }

        public void AddSyncComponents(
            dynamic graphLookupContent,
            Dictionary<string, object> nodeProperties,
            Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
//            GraphLookupPartSettings settings)
        {
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            JArray nodes = (JArray)graphLookupContent.Nodes;
            if (nodes.Count == 0)
                return;

            if (settings.RelationshipType == null)
            {
                foreach (JToken node in nodes)
                {
                    nodeProperties.Add("todo", node["Id"].ToString());
                }
            }
            else
            {
                nodeRelationships.Add(
                    (destNodeLabel: settings.NodeLabel!, destIdPropertyName: settings.ValueFieldName!,
                        relationshipType: settings.RelationshipType!), nodes.Select(n => n["Id"].ToString()));
            }
        }
    }
}
