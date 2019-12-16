using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => "GraphSyncPart";

        public void AddSyncComponents(
            dynamic graphSyncContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add(IdPropertyName, IdPropertyValue(graphSyncContent));
        }

        //todo: settable
        public string IdPropertyName => "uri";

        public string IdPropertyValue(dynamic graphSyncContent) => graphSyncContent.Text.ToString();
    }
}
