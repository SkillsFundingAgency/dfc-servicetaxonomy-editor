using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
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
        private List<CommandRelationship>? _removingRelationships;

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

        public virtual async Task AllowSync(
            JArray? contentItems,
            IGraphMergeContext context,
            IAllowSyncResult allowSyncResult)
        {
            List<CommandRelationship> requiredRelationships = await GetRequiredRelationshipsAndOptionallySync(contentItems, context, allowSyncResult);

            INodeAndOutRelationshipsAndTheirInRelationships? existing = (await context.GraphReplicaSet.Run(
                    new NodeAndOutRelationshipsAndTheirInRelationshipsQuery(
                        context.ReplaceRelationshipsCommand.SourceNodeLabels,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyName!,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyValue!)))
                .FirstOrDefault();

            if (existing?.OutgoingRelationships.Any() != true)
            {
                // nothing to do here, node is being newly created or existing node has no relationships
                return;
            }

            IEnumerable<string> embeddableContentTypes = GetEmbeddableContentTypes(context);

            var embeddedContentGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

            IEnumerable<string> relationshipTypes = await Task.WhenAll(
                embeddableContentTypes.Select(async ct =>
                {
                    embeddedContentGraphSyncHelper.ContentType = ct;
                    return await RelationshipType(embeddedContentGraphSyncHelper);
                }));

            existing = new NodeAndOutRelationshipsAndTheirInRelationships(
                existing.SourceNode,
                existing.OutgoingRelationships
                    .Where(or =>
                        embeddableContentTypes.Contains(
                            context.GraphSyncHelper.GetContentTypeFromNodeLabels(
                                or.outgoingRelationship.DestinationNode.Labels))
                        && relationshipTypes.Contains(or.outgoingRelationship.Relationship.Type)));

            IEnumerable<CommandRelationship> existingRelationshipsForEmbeddableContentTypes =
                existing.ToCommandRelationships(context.GraphSyncHelper);

            _removingRelationships = GetRemovingRelationships(
                existingRelationshipsForEmbeddableContentTypes,
                requiredRelationships,
                context.GraphSyncHelper);

            if (!_removingRelationships.Any())
            {
                // nothing to do here, not removing any relationships
                return;
            }

            foreach (var removingRelationship in _removingRelationships)
            {
                foreach (object destinationNodeIdPropertyValue in removingRelationship.DestinationNodeIdPropertyValues)
                {
                    var existingForRemoving = existing.OutgoingRelationships
                        .Where(er =>
                            er.outgoingRelationship.DestinationNode.Properties[
                                context.GraphSyncHelper.IdPropertyName(
                                    context.GraphSyncHelper.GetContentTypeFromNodeLabels(
                                        er.outgoingRelationship.DestinationNode.Labels))] ==
                            destinationNodeIdPropertyValue);

                    var nonTwoWayIncomingRelationshipsToEmbeddedItems = existingForRemoving
                        .SelectMany(or => or.incomingRelationships) //todo: null or throws?
                        .Where(ir => !ir.Relationship.Properties.ContainsKey(
                            NodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName));

                        allowSyncResult.AddSyncBlockers(
                            nonTwoWayIncomingRelationshipsToEmbeddedItems.Select(r =>
                                new SyncBlocker(
                                    context.GraphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNode.Labels),
                                    (string?)r.DestinationNode.Properties[TitlePartGraphSyncer.NodeTitlePropertyName])));
                }
            }
        }

        private async Task<List<CommandRelationship>> GetRequiredRelationshipsAndOptionallySync(
            JArray? contentItems,
            IGraphMergeContext context,
            IAllowSyncResult? allowSyncResult = null)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            List<CommandRelationship> requiredRelationships = new List<CommandRelationship>();

            int relationshipOrdinal = 0;
            foreach (ContentItem contentItem in embeddedContentItems)
            {
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                IAllowSyncResult embeddedAllowSyncResult = await mergeGraphSyncer.SyncAllowed(context.GraphReplicaSet, contentItem, context.ContentManager, context);
                allowSyncResult?.AddRelated(embeddedAllowSyncResult);
                if (embeddedAllowSyncResult.AllowSync != SyncStatus.Allowed)
                    continue;

                IMergeNodeCommand containedContentMergeNodeCommand = mergeGraphSyncer.MergeNodeCommand;

                containedContentMergeNodeCommand.CheckIsValid();

                if (allowSyncResult == null)
                {
                    // we're actually syncing, not checking if it's allowed
                    await mergeGraphSyncer.SyncToGraphReplicaSet();
                }
                else
                {
                    // we need to get the id
                    var graphSyncPartGraphSyncer = _serviceProvider.GetRequiredService<IGraphSyncPartGraphSyncer>();
                    graphSyncPartGraphSyncer.AddSyncComponents(contentItem.Content[nameof(GraphSyncPart)],
                        mergeGraphSyncer.GraphMergeContext);
                    //todo: delegate to mergeGraphSyncer then ContentItemGraphSyncer?
                }

                var embeddedContentItemGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                embeddedContentItemGraphSyncHelper.ContentType = contentItem.ContentType;

                string relationshipType = await RelationshipType(embeddedContentItemGraphSyncHelper);

                var properties = await GetRelationshipProperties(contentItem, relationshipOrdinal, context.GraphSyncHelper);
                ++relationshipOrdinal;

                requiredRelationships.Add(new CommandRelationship(
                    relationshipType,
                    await TwoWayIncomingRelationshipType(embeddedContentItemGraphSyncHelper),
                    properties,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName!,
                    Enumerable.Repeat(containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!], 1)));
            }

            return requiredRelationships;
        }

        public async Task AddSyncComponents(JArray? contentItems, IGraphMergeContext context)
        {
            List<CommandRelationship> requiredRelationships = await GetRequiredRelationshipsAndOptionallySync(contentItems, context);

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(requiredRelationships);

            await DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(context);
        }

        public async Task AllowDelete(
            JArray? contentItems,
            IGraphDeleteContext context,
            IAllowSyncResult allowSyncResult)
        {
            //todo: helper for common code
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            IEnumerable<string> embeddableContentTypes = GetEmbeddableContentTypes(context);

            var embeddedContentItemsByType = embeddedContentItems
                .GroupBy(ci => ci.ContentType)
                .Where(g => embeddableContentTypes.Contains(g.Key));

            foreach (var embeddedContentItemsOfType in embeddedContentItemsByType)
            {
                foreach (ContentItem contentItem in embeddedContentItemsOfType)
                {
                    IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

                    IAllowSyncResult embeddedAllowSyncResult = await deleteGraphSyncer.DeleteAllowed(
                        contentItem,
                        context.ContentItemVersion,
                        context.DeleteOperation,
                        context.DeleteIncomingRelationshipsProperties,
                        context);

                    allowSyncResult.AddRelated(embeddedAllowSyncResult);
                }
            }
        }

        //todo: best place for this to live?
        private static IEnumerable<KeyValuePair<string, object>> TwoWayRelationshipProperties { get; } =
            new Dictionary<string, object> {{NodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName, true}};

        public async Task DeleteComponents(JArray? contentItems, IGraphDeleteContext context)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            IEnumerable<string> embeddableContentTypes = GetEmbeddableContentTypes(context);

            var embeddedContentItemsByType = embeddedContentItems
                .GroupBy(ci => ci.ContentType)
                .Where(g => embeddableContentTypes.Contains(g.Key));

            foreach (var embeddedContentItemsOfType in embeddedContentItemsByType)
            {
                // var embeddedContentItemGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                // embeddedContentItemGraphSyncHelper.ContentType = embeddedContentItemsOfType.Key;

                //string? twoWayRelationshipType = await TwoWayIncomingRelationshipType(embeddedContentItemGraphSyncHelper);
                foreach (ContentItem contentItem in embeddedContentItemsOfType)
                {
                    IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();
                    //todo: incoming properties. pass parent?
                    //todo: return command, instead of executing it
                    //todo: unlike sync where the leaves need syncing first and then up the tree,
                    // delete needs to do it the other way, i.e. delete root first then down the tree
                    // breadth first or depth first doesn't matter
                    await deleteGraphSyncer.DeleteIfAllowed(
                        contentItem,
                        context.ContentItemVersion,
                        context.DeleteOperation,
                        TwoWayRelationshipProperties,
                        context);
                }

                // IGraphSyncHelper graphSyncHelperForType = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
                // graphSyncHelperForType.ContentType = embeddedContentItemsOfType.Key;
                //
                // var typesNodeLabels = await graphSyncHelperForType.NodeLabels();
                // string idPropertyName = graphSyncHelperForType.IdPropertyName();
                //
                // context.AddCommands(embeddedContentItemsOfType.Select(ci =>
                //     new DeleteNodeCommand
                //     {
                //         //todo: twoway for terms
                //         NodeLabels = new HashSet<string>(typesNodeLabels),
                //         IdPropertyName = idPropertyName,
                //         IdPropertyValue = graphSyncHelperForType.GetIdPropertyValue(
                //             ci.Content.GraphSyncPart, context.ContentItemVersion),
                //         DeleteNode = true
                //         //todo:?
                //         //DeleteIncomingRelationshipsProperties = deleteIncomingRelationshipsProperties;
                //     }));
            }
        }

        private async Task DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(IGraphMergeContext context)
        {
            if (_removingRelationships?.Any() != true)
            {
                // nothing to do here, not removing any relationships
                return;
            }

            //todo: copy ctor with bool to copy destid values? or existing relationships
            var deleteRelationshipCommand = new DeleteRelationshipsCommand
            {
                DeleteDestinationNodes = true,
                SourceNodeLabels = context.ReplaceRelationshipsCommand.SourceNodeLabels,
                SourceIdPropertyName = context.ReplaceRelationshipsCommand.SourceIdPropertyName,
                SourceIdPropertyValue = context.ReplaceRelationshipsCommand.SourceIdPropertyValue
            };
            deleteRelationshipCommand.AddRelationshipsTo(_removingRelationships);

            //todo: need to add command to context, or otherwise execute it
            // should add commands to be executed (in order) to context (same with embedded items)
            // so that everything syncs as a unit (atomically) or not within a transaction
            await context.GraphReplicaSet.Run(deleteRelationshipCommand);
        }

        private List<CommandRelationship> GetRemovingRelationships(
            IEnumerable<CommandRelationship> existing,
            IEnumerable<CommandRelationship> required,
            IGraphSyncHelper graphSyncHelper)
        {
            List<CommandRelationship> removingRelationships = new List<CommandRelationship>();

            var distinctExistingRelationshipsTypes = existing
                .Select(r => (r.RelationshipType,
                    DestinationNodeLabel: graphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels)))
                .Distinct();

            foreach (var distinctExistingRelationshipsType in distinctExistingRelationshipsTypes)
            {
                var (existingIdPropertyValues, existingRelationshipsOfType)
                    = GetIdPropertyValuesAndRelationshipsOfType(
                        existing, distinctExistingRelationshipsType, graphSyncHelper);
                if (!existingIdPropertyValues.Any())
                    continue;

                var (requiredIdPropertyValues, _) = GetIdPropertyValuesAndRelationshipsOfType(
                    required, distinctExistingRelationshipsType, graphSyncHelper);

                var removingIdPropertyValuesForRelationshipType = existingIdPropertyValues
                    .Except(requiredIdPropertyValues)
                    .ToArray();
                if (!removingIdPropertyValuesForRelationshipType.Any())
                    continue;

                var firstExistingRelationshipsOfType = existingRelationshipsOfType.First();
                removingRelationships.Add(new CommandRelationship(
                    // or we could use firstExistingRelationshipsOfType.RelationshipType,
                    distinctExistingRelationshipsType.RelationshipType,
                    null,
                    null,    // don't need properties for delete (plus they'd be different)
                    firstExistingRelationshipsOfType.DestinationNodeLabels,
                    firstExistingRelationshipsOfType.DestinationNodeIdPropertyName,
                    removingIdPropertyValuesForRelationshipType));
            }

            return removingRelationships;
        }

        private (object[] IdPropertyValues, CommandRelationship[] RelationshipsOfType)
            GetIdPropertyValuesAndRelationshipsOfType(
                IEnumerable<CommandRelationship> relationships,
                (string RelationshipType, string DestinationNodeLabel) relationshipType,
                IGraphSyncHelper graphSyncHelper)
        {
            var relationshipsOfType = relationships
                .Where(r =>
                    graphSyncHelper.GetContentTypeFromNodeLabels(r.DestinationNodeLabels) ==
                    relationshipType.DestinationNodeLabel)
                .ToArray();

            var idPropertyValues = relationshipsOfType
                .SelectMany(r => r.DestinationNodeIdPropertyValues)
                .ToArray();

            return (idPropertyValues, relationshipsOfType);
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

        protected abstract IEnumerable<string> GetEmbeddableContentTypes(IGraphOperationContext context);

        //todo: need to separate GraphSyncHelper into stateful and stateless
        protected virtual async Task<string> RelationshipType(IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            return await embeddedContentGraphSyncHelper.RelationshipTypeDefault(embeddedContentGraphSyncHelper.ContentType!);
        }

        protected virtual Task<string?> TwoWayIncomingRelationshipType(IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            return Task.FromResult(default(string));
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
