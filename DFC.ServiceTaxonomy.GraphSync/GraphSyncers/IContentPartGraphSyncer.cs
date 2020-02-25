using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IContentPartGraphSyncer
    {
        string? PartName {get;}

        //todo: new type(s) for relationships
        Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition);
    }
}
