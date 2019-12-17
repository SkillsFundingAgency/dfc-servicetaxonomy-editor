using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class TitlePartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => "TitlePart";

        public Task AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add("skos__prefLabel", graphLookupContent.Title.ToString());
            return Task.CompletedTask;
        }
    }
}
