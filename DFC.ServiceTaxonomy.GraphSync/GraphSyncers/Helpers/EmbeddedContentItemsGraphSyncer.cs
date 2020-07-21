using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public abstract class EmbeddedContentItemsGraphSyncer : IEmbeddedContentItemsGraphSyncer
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;

        protected EmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;

            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        public async Task AddSyncComponents(JArray? contentItems, IGraphMergeContext context)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            int relationshipOrdinal = 0;
            foreach (ContentItem contentItem in embeddedContentItems)
            {
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                IMergeNodeCommand? containedContentMergeNodeCommand = await mergeGraphSyncer.SyncToGraphReplicaSet(
                    context.GraphReplicaSet, contentItem, context.ContentManager);
                // if the contained content type wasn't synced (i.e. it doesn't have a graph sync part), then there's nothing to create a relationship to
                if (containedContentMergeNodeCommand == null)
                    continue;

                containedContentMergeNodeCommand.CheckIsValid();

                var embeddedContentItemGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                embeddedContentItemGraphSyncHelper.ContentType = contentItem.ContentType;

                string relationshipType = await RelationshipType(embeddedContentItemGraphSyncHelper);

                var properties = await GetRelationshipProperties(contentItem, relationshipOrdinal, context.GraphSyncHelper);
                ++relationshipOrdinal;

                //todo: if graphsyncpart text missing, return as null
                //todo: where uri null create relationship using displaytext instead
                //have fallback as flag, and only do it for taxonomy, or do it for all contained items?

                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    properties,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName!,
                    containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!]);
            }

            await DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(context, embeddedContentItems);
        }

        private async Task DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(
            IGraphMergeContext context,
            ContentItem[] embeddedContentItems)
        {
            IEnumerable<string> embeddableContentTypes =
                GetEmbeddableContentTypes(context.ContentItem, context.ContentTypePartDefinition);
            IEnumerable<string> embeddedContentTypes = embeddedContentItems
                .Select(i => i.ContentType)
                .Distinct(); // <= distinct is optional here

            IEnumerable<string> notEmbeddedContentTypes = embeddableContentTypes.Except(embeddedContentTypes);

            foreach (string notEmbeddedContentType in notEmbeddedContentTypes)
            {
                var notEmbeddedContentTypeGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                notEmbeddedContentTypeGraphSyncHelper.ContentType = notEmbeddedContentType;

                string relationshipType = await RelationshipType(notEmbeddedContentTypeGraphSyncHelper);

                IGraphSyncHelper graphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                graphSyncHelper.ContentType = notEmbeddedContentType;

                //todo: different method to make it obvious what we're doing here? EnsureRelationshipsAreDeleted
                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    null,
                    await graphSyncHelper.NodeLabels(notEmbeddedContentType),
                    graphSyncHelper.IdPropertyName(notEmbeddedContentType));
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JArray? contentItems,
            IValidateAndRepairContext context)
        {
            IEnumerable<ContentItem> embeddedContentItems = ConvertToContentItems(contentItems);

            int relationshipOrdinal = 0;
            foreach (ContentItem embeddedContentItem in embeddedContentItems)
            {
                ContentTypeDefinition embeddedContentTypeDefinition = _contentTypes[embeddedContentItem.ContentType];

                (bool validated, string failureReason) =
                    await context.ValidateAndRepairGraph.ValidateContentItem(
                        embeddedContentItem, embeddedContentTypeDefinition, context.ContentItemVersion);

                if (!validated)
                    return (false, $"contained item failed validation: {failureReason}");

                var embeddedContentGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                embeddedContentGraphSyncHelper.ContentType = embeddedContentItem.ContentType;

                // check expected relationship is in graph
                string expectedRelationshipType = await RelationshipType(embeddedContentGraphSyncHelper);

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.TryGetValue(expectedRelationshipType, out int currentCount);
                context.ExpectedRelationshipCounts[expectedRelationshipType] = ++currentCount;

                // we've already validated the destination node, so we can assume the id property is there
                object destinationId = embeddedContentGraphSyncHelper.GetIdPropertyValue(
                    embeddedContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string embeddedContentIdPropertyName = embeddedContentGraphSyncHelper.IdPropertyName(embeddedContentItem.ContentType);

                var expectedRelationshipProperties = await GetRelationshipProperties(
                    embeddedContentItem, relationshipOrdinal, embeddedContentGraphSyncHelper);
                ++relationshipOrdinal;

                (validated, failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithOutgoingRelationships,
                    expectedRelationshipType,
                    embeddedContentIdPropertyName,
                    destinationId,
                    expectedRelationshipProperties);

                if (!validated)
                    return (false, failureReason);
            }

            return (true, "");
        }

        protected abstract IEnumerable<string> GetEmbeddableContentTypes(
            ContentItem contentItem,
            ContentTypePartDefinition contentTypePartDefinition);

        protected virtual async Task<string> RelationshipType(IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            return await embeddedContentGraphSyncHelper.RelationshipTypeDefault(embeddedContentGraphSyncHelper.ContentType!);
        }

        protected virtual Task<Dictionary<string, object>?> GetRelationshipProperties(
            ContentItem contentItem,
            int ordinal,
            IGraphSyncHelper graphSyncHelper)
        {
            return Task.FromResult<Dictionary<string, object>?>(null);
        }

        private ContentItem[] ConvertToContentItems(JArray? contentItems)
        {
            if (contentItems == null)
            {
                // we've seen this when the import util generates bad data. we fail fast
                throw new GraphSyncException("Embedded content container has missing array.");
            }

            ContentItem[]? embeddedContentItems = contentItems.ToObject<ContentItem[]>();
            if (embeddedContentItems == null)
                throw new GraphSyncException("Embedded content container does not contain ContentItems.");
            return embeddedContentItems;
        }
    }
}
