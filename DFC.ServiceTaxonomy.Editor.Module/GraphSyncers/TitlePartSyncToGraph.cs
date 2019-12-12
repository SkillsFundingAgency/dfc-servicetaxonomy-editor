using System.Collections.Generic;
using DFC.ServiceTaxonomy.Editor.Module.Activities;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.GraphSyncers
{
    public class TitlePartSyncToGraph : ISyncPartToGraph
    {
        public string PartName
        {
            get { return "TitlePart"; }
        }

        public void AddSyncComponents(
            dynamic graphLookupContent,
            Dictionary<string, object> nodeProperties,
            Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add("skos__prefLabel", graphLookupContent.Title.ToString());
        }
    }
}
