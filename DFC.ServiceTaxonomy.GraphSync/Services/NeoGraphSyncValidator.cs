using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Neo4j.Driver;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class NeoGraphSyncValidator : IGraphSyncValidator
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphDatabase _graphDatabase;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;
        private readonly Dictionary<string, IContentPartGraphSyncer> _partSyncers;

        public NeoGraphSyncValidator(
            IContentDefinitionManager contentDefinitionManager,
            IGraphDatabase graphDatabase,
            IEnumerable<IContentPartGraphSyncer> partSyncers)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _graphDatabase = graphDatabase;
            _partSyncers = partSyncers.ToDictionary(x => x.PartName ?? "Eponymous");
            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        public async Task<bool> CheckIfContentItemSynced(ContentItem contentItem)
        {
            var results = await _graphDatabase.Run(new MatchNodeWithAllOutgoingRelationshipsQuery(contentItem.ContentType, (string)contentItem.Content.GraphSyncPart.Text));

            if (results == null || !results.Any())
            {
                return false;
            }
            else
            {
                var contentDefinition = _contentTypes[contentItem.ContentType];

                var sourceNode = results.Select(x => x[0]).Cast<INode>().First();
                var relationships = results.Select(x => x[1]).Cast<IRelationship>().ToList();
                var destNodes = results.Select(x => x[2]).Cast<INode>().ToList();

                //for some reason sometimes we get an array with a single null element
                relationships.RemoveAll(x => x == null);

                foreach (var part in contentDefinition.Parts)
                {
                    if (!_partSyncers.TryGetValue(part.PartDefinition.Name, out var partSyncer))
                    {
                        partSyncer = _partSyncers["Eponymous"];
                    }

                    if (!await partSyncer.VerifySyncComponent(contentItem, part, sourceNode, relationships, destNodes))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
