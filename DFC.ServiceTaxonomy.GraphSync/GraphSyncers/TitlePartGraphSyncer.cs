using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class TitlePartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(TitlePart);

        public Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            //IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add("skos__prefLabel", graphLookupContent.Title.ToString());

            return Task.FromResult(Enumerable.Empty<Query>());
        }
    }
}
