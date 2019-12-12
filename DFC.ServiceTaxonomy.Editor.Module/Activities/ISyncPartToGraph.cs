using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.Activities
{
    //public interface ISyncPartToGraph<TPartSettings>
    public interface ISyncPartToGraph
    {
    string PartName {get;}


        void AddSyncComponents(dynamic graphLookup, Dictionary<string, object> nodeProperties,
            Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
    ContentTypePartDefinition contentTypePartDefinition);
    //            TPartSettings settings);
    }
}
