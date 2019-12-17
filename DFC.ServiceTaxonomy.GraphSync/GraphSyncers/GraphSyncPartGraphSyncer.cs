using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => "GraphSyncPart";

        public Task AddSyncComponents(
            dynamic graphSyncContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add(IdPropertyName, IdPropertyValue(graphSyncContent));
            return Task.CompletedTask;
        }

        //todo: settable
        public string IdPropertyName => "uri";

        public string IdPropertyValue(dynamic graphSyncContent) => graphSyncContent.Text.ToString();
    }
}
