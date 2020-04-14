using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BagPartGraphSyncer> _logger;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider,
            ILogger<BagPartGraphSyncer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            foreach (JObject? contentItem in content.ContentItems)
            {
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                string contentItemId = contentItem!["ContentItemId"]!.ToString();
                string contentItemVersionId = contentItem!["ContentItemVersionId"]!.ToString();

                DateTime? createdDate = !string.IsNullOrEmpty(contentItem["CreatedUtc"]!.ToString()) ? DateTime.Parse(contentItem["CreatedUtc"]!.ToString()) : (DateTime?)null;
                DateTime? modifiedDate = !string.IsNullOrEmpty(contentItem["ModifiedUtc"]!.ToString()) ? DateTime.Parse(contentItem["ModifiedUtc"]!.ToString()) : (DateTime?)null;

                //todo: if we want to support nested bags, would have to return queries also
                IMergeNodeCommand? containedContentMergeNodeCommand = await mergeGraphSyncer.SyncToGraph(
                    contentType,
                    contentItemId,
                    contentItemVersionId,
                    contentItem!,
                    createdDate,
                    modifiedDate);
                // if the contained content type wasn't synced (i.e. it doesn't have a graph sync part), then there's nothing to create a relationship to
                if (containedContentMergeNodeCommand == null)
                    continue;

                containedContentMergeNodeCommand.CheckIsValid();

                var bagContentItemGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

                bagContentItemGraphSyncHelper.ContentType = contentType;
                string relationshipType = await RelationshipType(bagContentItemGraphSyncHelper);

                replaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName!,
                    containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!]);
            }
        }

        public async Task<bool> VerifySyncComponent(
            dynamic content,
            ContentTypePartDefinition contentTypePartDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper)
        {
            IEnumerable<ContentItem> contentItems = content["ContentItems"].ToObject<IEnumerable<ContentItem>>();

            Dictionary<string, int> expectedRelationshipCounts = new Dictionary<string, int>();

            foreach (ContentItem bagPartContentItem in contentItems)
            {
                var graphSyncValidator = _serviceProvider.GetRequiredService<IValidateAndRepairGraph>();

                ContentTypeDefinition bagPartContentTypeDefinition = _contentTypes[bagPartContentItem.ContentType];

                //todo: check failed bag items get repair attempt
                if (!await graphSyncValidator.ValidateContentItem(bagPartContentItem, bagPartContentTypeDefinition))
                    return false;

                // check expected relationship is in graph
                var bagContentGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

                bagContentGraphSyncHelper.ContentType = bagPartContentItem.ContentType;
                string expectedRelationshipType = await RelationshipType(bagContentGraphSyncHelper);

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.TryGetValue(expectedRelationshipType, out int currentCount);
                expectedRelationshipCounts[expectedRelationshipType] = ++currentCount;

                object destinationId = bagContentGraphSyncHelper.GetIdPropertyValue(bagPartContentItem.Content.GraphSyncPart);

                INode destinationNode = destinationNodes.SingleOrDefault(n => Equals(n.Properties[graphSyncHelper.IdPropertyName()], destinationId));
                if (destinationNode == null)
                {
                    _logger.LogWarning($"Sync validation failed. Destination node with user ID '{destinationId}' not found");
                    return false;
                }

                var relationship = relationships.SingleOrDefault(r =>
                    r.Type == expectedRelationshipType && r.EndNodeId == destinationNode.Id);

                if (relationship == null)
                {
                    _logger.LogWarning($"Sync validation failed. Relationship of type {expectedRelationshipType} with end node ID {destinationNode.Id} not found");
                    return false;
                }
            }

            // check there aren't any more relationships of each type than there should be
            foreach ((string relationshipType, int relationshipsInDbCount) in expectedRelationshipCounts)
            {
                int relationshipsInGraphCount = relationships.Count(r => r.Type == relationshipType);

                //todo: this fails if the content type has >1 part that creates relationships of the same type
                // do we cater for that eventuality, or remove this check?
                //if we remove this check we'll miss extra relationships that shouldn't be there
                // it might be best to check what should be there and ignore anything extra
                // (similar question about extra properties)
                if (relationshipsInDbCount != relationshipsInGraphCount)
                {
                    _logger.LogWarning($"Sync validation failed. Expecting {relationshipsInDbCount} relationships of type {relationshipType} in graph, but found {relationshipsInGraphCount}");
                    return false;
                }
            }

            return true;
        }

        private async Task<string> RelationshipType(IGraphSyncHelper graphSyncHelper)
        {
            //todo: what if want different relationships for same contenttype in different bags!
            string? relationshipType = graphSyncHelper.GraphSyncPartSettings.BagPartContentItemRelationshipType;
            if (string.IsNullOrEmpty(relationshipType))
                relationshipType = await graphSyncHelper.RelationshipTypeDefault(graphSyncHelper.ContentType!);

            return relationshipType;
        }
    }
}
