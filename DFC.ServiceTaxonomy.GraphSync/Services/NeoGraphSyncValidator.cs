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
        private readonly IGraphDatabase _graphDatabase;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;
        private readonly Dictionary<string, IContentPartGraphSyncer> _partSyncers;

        public NeoGraphSyncValidator(
            IContentDefinitionManager contentDefinitionManager,
            IGraphDatabase graphDatabase,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncHelper graphSyncHelper)
        {
            _graphDatabase = graphDatabase;
            _graphSyncHelper = graphSyncHelper;
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
                return false;

            var contentDefinition = _contentTypes[contentItem.ContentType];

            var sourceNode = results.Select(x => x[0]).Cast<INode>().First();
            var relationships = results.Select(x => x[1]).Cast<IRelationship>().ToList();
            var destNodes = results.Select(x => x[2]).Cast<INode>().ToList();

            //for some reason sometimes we get an array with a single null element
            relationships.RemoveAll(x => x == null);

            _graphSyncHelper.ContentType = contentItem.ContentType;

            foreach (var part in contentDefinition.Parts)
            {
                if (!_partSyncers.TryGetValue(part.PartDefinition.Name, out var partSyncer))
                {
                    partSyncer = _partSyncers["Eponymous"];
                }
                
                if (!await partSyncer.VerifySyncComponent(contentItem, part, sourceNode, relationships, destNodes, _graphSyncHelper))
                    return false;
            }

            return true;
        }
    }
}
