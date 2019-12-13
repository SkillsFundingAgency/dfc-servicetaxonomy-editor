using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.Activities
{
    public interface IContentPartGraphSyncer
    {
        string PartName {get;}

        //todo: new type(s) for relationships
        void AddSyncComponents(dynamic graphLookupContent, Dictionary<string, object> nodeProperties,
            Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition);
    }
}
