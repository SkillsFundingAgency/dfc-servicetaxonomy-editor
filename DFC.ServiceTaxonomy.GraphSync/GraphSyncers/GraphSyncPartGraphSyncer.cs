using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class GraphSyncPartGraphSyncer
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
