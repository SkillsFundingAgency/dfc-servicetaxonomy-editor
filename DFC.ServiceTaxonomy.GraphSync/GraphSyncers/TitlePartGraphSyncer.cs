using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class TitlePartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName
        {
            get { return "TitlePart"; }
        }

        public void AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add("skos__prefLabel", graphLookupContent.Title.ToString());
        }
    }
}
