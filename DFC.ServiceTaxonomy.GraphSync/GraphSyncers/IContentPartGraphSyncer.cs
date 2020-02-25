using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
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
            //IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition);
    }
}
