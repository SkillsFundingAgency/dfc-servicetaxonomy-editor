using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class ValidateGraphSync : IValidateGraphSync
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly ILogger<ValidateGraphSync> _logger;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;
        private readonly Dictionary<string, IContentPartGraphSyncer> _partSyncers;

        public ValidateGraphSync(
            IContentDefinitionManager contentDefinitionManager,
            IGraphDatabase graphDatabase,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncHelper graphSyncHelper,
            ILogger<ValidateGraphSync> logger)
        {
            _graphDatabase = graphDatabase;
            _graphSyncHelper = graphSyncHelper;
            _logger = logger;
            _partSyncers = partSyncers.ToDictionary(x => x.PartName ?? "Eponymous");
            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        public async Task<bool> CheckIfContentItemSynced(ContentItem contentItem)
        {
            _graphSyncHelper.ContentType = contentItem.ContentType;

            string contentItemId = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart);

            List<IRecord> results = await _graphDatabase.Run(new MatchNodeWithAllOutgoingRelationshipsQuery(
                await _graphSyncHelper.NodeLabels(),
                _graphSyncHelper.IdPropertyName,
                contentItemId));

            if (results == null || !results.Any())
                return false;

            var contentDefinition = _contentTypes[contentItem.ContentType];

            INode? sourceNode = results.Select(x => x[0]).Cast<INode?>().FirstOrDefault();
            if (sourceNode == null)
                return false;

            List<IRelationship> relationships = results.Select(x => x[1]).Cast<IRelationship>().ToList();
            List<INode> destinationNodes = results.Select(x => x[2]).Cast<INode>().ToList();

            //for some reason sometimes we get an array with a single null element
            relationships.RemoveAll(x => x == null);

            foreach (var part in contentDefinition.Parts)
            {
                string partName = part.PartDefinition.Name;
                if (!_partSyncers.TryGetValue(partName, out var partSyncer))
                {
                    partSyncer = _partSyncers["Eponymous"];
                }

                dynamic? partContent = contentItem.Content[partName];
                if (partContent == null)
                    continue; //todo: throw??

                if (!await partSyncer.VerifySyncComponent(
                    partContent, part,
                    sourceNode, relationships, destinationNodes,
                    _graphSyncHelper))
                {
                    LogValidationFailure(contentItemId, contentItem, partName, partContent, sourceNode);
                    return false;
                }
            }

            return true;
        }

        private void LogValidationFailure(string contentItemId, ContentItem contentItem, string partName, dynamic partContent, INode sourceNode)
        {
            _logger.LogWarning($@"Sync validation failed.
Content type: '{contentItem.ContentType}'
Content item ID: '{contentItemId}'
Content part type name: 'todo'
             name '{partName}'
             content: '{partContent}'
Source node ID: {sourceNode.Id}
            labels: ':{string.Join(":", sourceNode.Labels)}'
            properties: '{string.Join(",", sourceNode.Properties.Select(p => $"{p.Key}={p.Value}"))}'");
        }
    }
}
