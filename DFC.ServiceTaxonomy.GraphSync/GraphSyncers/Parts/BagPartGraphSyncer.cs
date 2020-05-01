using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

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

        //todo: rename to ValidateSyncComponent
        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            IEnumerable<ContentItem>? contentItems = content["ContentItems"]?.ToObject<IEnumerable<ContentItem>>();
            if (contentItems == null)
                throw new GraphSyncException("Bag does not contain ContentItems");

            foreach (ContentItem bagPartContentItem in contentItems)
            {
                var graphSyncValidator = _serviceProvider.GetRequiredService<IValidateAndRepairGraph>();

                ContentTypeDefinition bagPartContentTypeDefinition = _contentTypes[bagPartContentItem.ContentType];

                (bool validated, string failureReason) =
                    await graphSyncValidator.ValidateContentItem(bagPartContentItem, bagPartContentTypeDefinition, endpoint);

                if (!validated)
                    return (false, $"contained item failed validation: {failureReason}");

                // check expected relationship is in graph
                var bagContentGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

                bagContentGraphSyncHelper.ContentType = bagPartContentItem.ContentType;
                string expectedRelationshipType = await RelationshipType(bagContentGraphSyncHelper);

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.TryGetValue(expectedRelationshipType, out int currentCount);
                expectedRelationshipCounts[expectedRelationshipType] = ++currentCount;

                // we've already validated the destination node, so we can assume the id property is there
                object destinationId = bagContentGraphSyncHelper.GetIdPropertyValue(bagPartContentItem.Content.GraphSyncPart);

                string bagContentIdPropertyName = bagContentGraphSyncHelper.IdPropertyName(bagPartContentItem.ContentType);

                (validated, failureReason) = graphValidationHelper.ValidateOutgoingRelationship(
                    nodeWithOutgoingRelationships,
                    expectedRelationshipType,
                    bagContentIdPropertyName,
                    destinationId);

                if (!validated)
                    return (false, failureReason);
            }

            return (true, "");
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
