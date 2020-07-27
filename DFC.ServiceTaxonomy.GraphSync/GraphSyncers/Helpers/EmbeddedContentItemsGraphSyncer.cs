using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    //todo: need to handle this example scenario:
    // 1 page has preview version that is only user of a page location
    // user deletes that page location and published taxonomy
    // preview graph sync fails (correctly) as location is used by preview page
    // published sync works as no page uses location
    // we need to disable the publish of the location into oc's database
    // but that would still leave the published graph without the location.
    // we need to cancel the whole sync to both graphs too
    // which means we can't work on the graph as the sync is ongoing
    // instead we'll need to supply an ordered list of commands to the context
    // and execute them all, or none of them
    // (we should be doing that anyway so that a sync is atomic)
    // issue is sync to both published and preview graph needs to be atomic
    // how do we handle one graph succeeding and one failing?
    // neo looking into recovery point feature, but not yet supported
    // compensating transaction would be painful
    // is there any support for a 2 phase commit? https://groups.google.com/forum/#!topic/neo4j/E2HvHViX8Ac

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

            List<CommandRelationship> requiredRelationships = new List<CommandRelationship>();

            int relationshipOrdinal = 0;
            foreach (ContentItem contentItem in embeddedContentItems)
            {
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                IMergeNodeCommand? containedContentMergeNodeCommand = await mergeGraphSyncer.SyncToGraphReplicaSet(
                    context.GraphReplicaSet,
                    contentItem,
                    context.ContentManager,
                    context);
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

                requiredRelationships.Add(new CommandRelationship(
                    relationshipType,
                    properties,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName!,
                    Enumerable.Repeat(containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!], 1)));
            }

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(requiredRelationships);

            await DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(context, requiredRelationships);
        }

        private async Task DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(
            IGraphMergeContext context,
            List<CommandRelationship> requiredRelationships)
        {
            INodeWithOutgoingRelationships? existingGraphSync = (await context.GraphReplicaSet.Run(
                    new NodeWithOutgoingRelationshipsQuery(
                        context.ReplaceRelationshipsCommand.SourceNodeLabels,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyName!,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyValue!)))
                .FirstOrDefault();

            //either work off content items, or what added to replacerelationship command

            if (existingGraphSync == null)    // nothing to do here, node is being newly created
                return;

            IEnumerable<CommandRelationship> existingRelationships = existingGraphSync.ToCommandRelationships(context.GraphSyncHelper);

            IEnumerable<string> embeddableContentTypes = GetEmbeddableContentTypes(context);

            var existingRelationshipsForEmbeddableContentTypes = existingRelationships
                .Where(r => embeddableContentTypes.Contains(
                    context.GraphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels)));

            var removingRelationships = GetRemovingRelationships(
                existingRelationshipsForEmbeddableContentTypes,
                requiredRelationships,
                context.GraphSyncHelper);

            if (!removingRelationships.Any())    // nothing to do here, not removing any relationships
                return;

            //todo: copy ctor with bool to copy destid values? or existing relationships
            var command = new ReplaceRelationshipsCommand();
            command.SourceNodeLabels = context.ReplaceRelationshipsCommand.SourceNodeLabels;
            command.SourceIdPropertyName = context.ReplaceRelationshipsCommand.SourceIdPropertyName;
            command.SourceIdPropertyValue = context.ReplaceRelationshipsCommand.SourceIdPropertyValue;
            command.AddRelationshipsTo(removingRelationships);
            var deleteRelationshipCommand = command.GetDeleteRelationshipsCommand(true);

            //todo: need to add command to context, or otherwise execute it
            // should add commands to be executed (in order) to context (same with embedded items)
            // so that everything syncs as a unit or not within a transaction
            await context.GraphReplicaSet.Run(deleteRelationshipCommand);

            // IEnumerable<string> embeddedContentTypes = embeddedContentItems
            //     .Select(i => i.ContentType)
            //     .Distinct(); // <= distinct is optional here
            //
            // IEnumerable<string> notEmbeddedContentTypes = embeddableContentTypes.Except(embeddedContentTypes);
            //
            // foreach (string notEmbeddedContentType in notEmbeddedContentTypes)
            // {
            //     var notEmbeddedContentTypeGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
            //     notEmbeddedContentTypeGraphSyncHelper.ContentType = notEmbeddedContentType;
            //
            //     string relationshipType = await RelationshipType(notEmbeddedContentTypeGraphSyncHelper);
            //
            //     IGraphSyncHelper graphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
            //     graphSyncHelper.ContentType = notEmbeddedContentType;
            //
            //     context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
            //         relationshipType,
            //         null,
            //         await graphSyncHelper.NodeLabels(notEmbeddedContentType),
            //         graphSyncHelper.IdPropertyName(notEmbeddedContentType));
            // }
        }

        // RelationshipExcept
        // private List<CommandRelationship> GetRemovingRelationships(
        //     IEnumerable<CommandRelationship> existing,
        //     IEnumerable<CommandRelationship> required,
        //     IGraphSyncHelper graphSyncHelper)
        // {
        //     List<CommandRelationship> removingRelationships = new List<CommandRelationship>();
        //
        //     foreach (var existingRelationship in existing)
        //     {
        //         var matchingRequiredRelationships = required
        //             .Where(r =>
        //             r.RelationshipType == existingRelationship.RelationshipType &&
        //             // should really do orderby, sequenceeqals, orderby instead
        //             //todo: check if the generated relationshipcommand needs to do a sequence equals too
        //             //r.DestinationNodeLabels.Equals(existingRelationship.DestinationNodeLabels))
        //             graphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels) == graphSyncHelper.GetContentTypeFromNodeLabels(existingRelationship.DestinationNodeLabels))
        //             .ToArray();
        //
        //         if (matchingRequiredRelationships.Any())
        //         {
        //             //todo: another groupby like ToCommandRelationships
        //             //todo: should clone really
        //             //CommandRelationship partialExistingRelationships = new CommandRelationship(existingRelationship);
        //
        //             var firstMatchingRequiredRelationship = matchingRequiredRelationships.First();
        //
        //             IEnumerable<object> partialExistingRelationshipIdValues =
        //                 existingRelationship.DestinationNodeIdPropertyValues;
        //
        //             foreach (var matchingRequiredRelationship in matchingRequiredRelationships)
        //             {
        //                 partialExistingRelationshipIdValues = partialExistingRelationshipIdValues.Except(
        //                     matchingRequiredRelationship.DestinationNodeIdPropertyValues);
        //
        //                 // existingRelationship.DestinationNodeIdPropertyValues = existingRelationship.DestinationNodeIdPropertyValues.Except(matchingRequiredRelationship
        //                 //     .DestinationNodeIdPropertyValues);
        //             }
        //
        //             removingRelationships.Add(new CommandRelationship(
        //                 firstMatchingRequiredRelationship.RelationshipType,
        //                 firstMatchingRequiredRelationship.Properties,
        //                 firstMatchingRequiredRelationship.DestinationNodeLabels,
        //                 firstMatchingRequiredRelationship.DestinationNodeIdPropertyName,
        //                 partialExistingRelationshipIdValues));
        //         }
        //         else
        //         {
        //             removingRelationships.Add(existingRelationship);
        //         }
        //     }
        //
        //     return removingRelationships;
        // }

        private List<CommandRelationship> GetRemovingRelationships(
            IEnumerable<CommandRelationship> existing,
            IEnumerable<CommandRelationship> required,
            IGraphSyncHelper graphSyncHelper)
        {
            List<CommandRelationship> removingRelationships = new List<CommandRelationship>();
//todo: this has effectively already been done
            var distinctExistingRelationshipsTypes = existing
                .Select(r => (r.RelationshipType,
                    DestinationNodeLabel: graphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels)))
                .Distinct();

            foreach (var distinctExistingRelationshipsType in distinctExistingRelationshipsTypes)
            {
                var existingRelationshipsOfType = existing
                    .Where(r =>
                        r.RelationshipType == distinctExistingRelationshipsType.RelationshipType &&
                        // should really do orderby, sequenceeqals, orderby instead
                        //todo: check if the generated relationshipcommand needs to do a sequence equals too
                        //r.DestinationNodeLabels.Equals(existingRelationship.DestinationNodeLabels))
                        graphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels) == distinctExistingRelationshipsType.DestinationNodeLabel)
                    .ToArray();

                var existingIdPropertyValues = existingRelationshipsOfType.SelectMany(r => r.DestinationNodeIdPropertyValues);
                if (!existingIdPropertyValues.Any())
                    continue;

                var requiredRelationshipsOfType = required
                    .Where(r =>
                        r.RelationshipType == distinctExistingRelationshipsType.RelationshipType &&
                        // should really do orderby, sequenceeqals, orderby instead
                        //todo: check if the generated relationshipcommand needs to do a sequence equals too
                        //r.DestinationNodeLabels.Equals(existingRelationship.DestinationNodeLabels))
                        graphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels) == distinctExistingRelationshipsType.DestinationNodeLabel)
                    .ToArray();

                var requiredIdPropertyValues = requiredRelationshipsOfType.SelectMany(r => r.DestinationNodeIdPropertyValues);

                var removingIdPropertyValuesForRelationshipType = existingIdPropertyValues.Except(requiredIdPropertyValues);
                if (!removingIdPropertyValuesForRelationshipType.Any())
                    continue;

                var firstExistingRelationshipsOfType = existingRelationshipsOfType.First();
                removingRelationships.Add(new CommandRelationship(
                    //firstExistingRelationshipsOfType.RelationshipType,
                    distinctExistingRelationshipsType.RelationshipType,
                    null,    // don't need properties for delete (plus they'd be different)
                    firstExistingRelationshipsOfType.DestinationNodeLabels,
                    firstExistingRelationshipsOfType.DestinationNodeIdPropertyName,
                    removingIdPropertyValuesForRelationshipType));
            }

            return removingRelationships;
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

        protected abstract IEnumerable<string> GetEmbeddableContentTypes(IGraphMergeContext context);

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
