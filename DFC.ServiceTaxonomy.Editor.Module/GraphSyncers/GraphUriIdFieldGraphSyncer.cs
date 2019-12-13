using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.GraphSyncers
{
    //todo: once we swap graph field to graph sync part
    public class GraphUriIdFieldGraphSyncer
    {
        public string PartName
        {
            get { return "GraphSyncPart"; }
        }

        public void AddSyncComponents(
            dynamic graphSyncContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add("uri", graphSyncContent.UriId.ToString());
        }
    }
}
