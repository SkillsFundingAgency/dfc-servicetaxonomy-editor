using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class TitlePartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(TitlePart);

        public Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            mergeNodeCommand.Properties.Add("skos__prefLabel", graphLookupContent.Title.ToString());

            return Task.FromResult(Enumerable.Empty<Query>());
        }

        public Task<bool> VerifySyncComponent(ContentItem contentItem, INode node,
            ContentTypePartDefinition contentTypePartDefinition, IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes)
        {
            var prefLabel = node.Properties["skos__prefLabel"];
            return Task.FromResult(Convert.ToString(prefLabel) == Convert.ToString(contentItem.Content.TitlePart.Title));
        }
    }
}
