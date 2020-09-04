using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public abstract class EmbeddedContentItemsGraphSyncer : IEmbeddedContentItemsGraphSyncer
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly ISyncNameProvider _statelessSyncNameProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;
        private List<CommandRelationship>? _removingRelationships;

        protected EmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            ISyncNameProvider statelessSyncNameProvider,
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _statelessSyncNameProvider = statelessSyncNameProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;

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
            _logger.LogDebug("Do embedded items allow sync?");

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

            (string[] embeddableContentTypes, IEnumerable<string> relationshipTypes) =
                await GetEmbeddableContentTypesAndRelationshipTypes(context);

            existing = new NodeAndOutRelationshipsAndTheirInRelationships(
                existing.SourceNode,
                existing.OutgoingRelationships
                    .Where(or =>
                        embeddableContentTypes.Contains(
                            context.SyncNameProvider.GetContentTypeFromNodeLabels(
                                or.outgoingRelationship.DestinationNode.Labels))
                        && relationshipTypes.Contains(or.outgoingRelationship.Relationship.Type)));

            IEnumerable<CommandRelationship> existingRelationshipsForEmbeddableContentTypes =
                existing.ToCommandRelationships(context.SyncNameProvider);

            _removingRelationships = GetRemovingRelationships(
                existingRelationshipsForEmbeddableContentTypes,
                requiredRelationships,
                context.SyncNameProvider);

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
                                context.SyncNameProvider.IdPropertyName(
                                    context.SyncNameProvider.GetContentTypeFromNodeLabels(
                                        er.outgoingRelationship.DestinationNode.Labels))] ==
                            destinationNodeIdPropertyValue);

                    var nonTwoWayIncomingRelationshipsToEmbeddedItems = existingForRemoving
                        .SelectMany(or => or.incomingRelationships) //todo: null or throws?
                        .Where(ir => !ir.Relationship.Properties.ContainsKey(
                            NodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName));

                    allowSyncResult.AddSyncBlockers(
                        nonTwoWayIncomingRelationshipsToEmbeddedItems.Select(r =>
                            new SyncBlocker(
                                context.SyncNameProvider.GetContentTypeFromNodeLabels(r.DestinationNode.Labels),
                                (string?)r.DestinationNode.Properties[TitlePartGraphSyncer.NodeTitlePropertyName])));
                }
            }
        }

        private IMergeGraphSyncer GetNewMergeGraphSyncer()
        {
            _logger.LogDebug("Getting new IMergeGraphSyncer.");

            return _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
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
                IMergeGraphSyncer? mergeGraphSyncer;

                if (allowSyncResult == null)
                {
                    // we're actually syncing, not checking if it's allowed
                    mergeGraphSyncer = await context.MergeGraphSyncer.SyncEmbedded(contentItem);
                }
                else
                {
                    mergeGraphSyncer = GetNewMergeGraphSyncer();

                    IAllowSyncResult embeddedAllowSyncResult = await mergeGraphSyncer.SyncAllowed(context.GraphReplicaSet, contentItem, context.ContentManager, context);
                    allowSyncResult.AddRelated(embeddedAllowSyncResult);
                    if (embeddedAllowSyncResult.AllowSync != SyncStatus.Allowed)
                        continue;
                }

                //todo: check embedded items with no graphsyncpart attached
                if (mergeGraphSyncer == null)
                    continue;

                IMergeNodeCommand containedContentMergeNodeCommand = mergeGraphSyncer.MergeNodeCommand;
                containedContentMergeNodeCommand.CheckIsValid();

                string relationshipType = await RelationshipType(contentItem.ContentType);

                var properties = await GetRelationshipProperties(contentItem, relationshipOrdinal, context.SyncNameProvider);
                ++relationshipOrdinal;

                requiredRelationships.Add(new CommandRelationship(
                    relationshipType,
                    await TwoWayIncomingRelationshipType(contentItem.ContentType),
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

            DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(context);
        }

        public async Task AllowSyncDetaching(IGraphMergeContext context, IAllowSyncResult allowSyncResult)
        {
            //todo: factor out common code

            INodeAndOutRelationshipsAndTheirInRelationships? existing = (await context.GraphReplicaSet.Run(
                    new NodeAndOutRelationshipsAndTheirInRelationshipsQuery(
                        context.ReplaceRelationshipsCommand.SourceNodeLabels,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyName!,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyValue!)))
                .FirstOrDefault();

            if (existing?.OutgoingRelationships.Any() != true)
            {
                // nothing to do here, existing node has no relationships
                return;
            }

            (string[] embeddableContentTypes, IEnumerable<string> relationshipTypes) =
                await GetEmbeddableContentTypesAndRelationshipTypes(context);

            existing = new NodeAndOutRelationshipsAndTheirInRelationships(
                existing.SourceNode,
                existing.OutgoingRelationships
                    .Where(or =>
                        embeddableContentTypes.Contains(
                            context.SyncNameProvider.GetContentTypeFromNodeLabels(
                                or.outgoingRelationship.DestinationNode.Labels))
                        && relationshipTypes.Contains(or.outgoingRelationship.Relationship.Type)));

            IEnumerable<CommandRelationship> existingRelationshipsForEmbeddableContentTypes =
                existing.ToCommandRelationships(context.SyncNameProvider);

            //todo: might be able to simplify this code
            foreach (var removingRelationship in existingRelationshipsForEmbeddableContentTypes)
            {
                foreach (object destinationNodeIdPropertyValue in removingRelationship.DestinationNodeIdPropertyValues)
                {
                    var existingForRemoving = existing.OutgoingRelationships
                        .Where(er =>
                            er.outgoingRelationship.DestinationNode.Properties[
                                context.SyncNameProvider.IdPropertyName(
                                    context.SyncNameProvider.GetContentTypeFromNodeLabels(
                                        er.outgoingRelationship.DestinationNode.Labels))] ==
                            destinationNodeIdPropertyValue);

                    var nonTwoWayIncomingRelationshipsToEmbeddedItems = existingForRemoving
                        .SelectMany(or => or.incomingRelationships) //todo: null or throws?
                        .Where(ir => !ir.Relationship.Properties.ContainsKey(
                            NodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName));

                        allowSyncResult.AddSyncBlockers(
                            nonTwoWayIncomingRelationshipsToEmbeddedItems.Select(r =>
                                new SyncBlocker(
                                    context.SyncNameProvider.GetContentTypeFromNodeLabels(r.DestinationNode.Labels),
                                    (string?)r.DestinationNode.Properties[TitlePartGraphSyncer.NodeTitlePropertyName])));
                }
            }
        }

        public async Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            // delete all relationships of all embeddable relationship types and their destination nodes
            // (and the destination node's outgoing relationships and incoming two-way relationships)

            //todo: put serviceprovider into the context??
            var deleteRelationshipsCommand = _serviceProvider.GetRequiredService<IDeleteRelationshipsCommand>();

            //todo: method on deleteRelationshipsCommand for this??
            deleteRelationshipsCommand.DeleteDestinationNodes = true;
            deleteRelationshipsCommand.SourceNodeLabels = new HashSet<string>(context.ReplaceRelationshipsCommand.SourceNodeLabels);
            deleteRelationshipsCommand.SourceIdPropertyName = context.ReplaceRelationshipsCommand.SourceIdPropertyName;
            deleteRelationshipsCommand.SourceIdPropertyValue =
                context.ReplaceRelationshipsCommand.SourceIdPropertyValue;

            context.ExtraCommands.Add(deleteRelationshipsCommand);

            (string[] embeddableContentTypes, IEnumerable<string> relationshipTypes) =
                await GetEmbeddableContentTypesAndRelationshipTypes(context);

            var possibleRelationships = relationshipTypes
                .Zip(embeddableContentTypes, (rt, ct) => new RelationshipTypeToContentType(rt, ct));

            foreach (var possibleRelationship in possibleRelationships)
            {
                //todo: add a RemoveAnyRelationshipsTo overload?
                deleteRelationshipsCommand.AddRelationshipsTo(possibleRelationship.RelationshipType,
                    null,
                    await context.SyncNameProvider.NodeLabels(possibleRelationship.ContentType),
                    null);
            }
        }

        //todo: better name, or zip into ienumerable of tuple
        private class RelationshipTypeToContentType
        {
            public string RelationshipType { get; set; }
            public string ContentType { get; set; }

            public RelationshipTypeToContentType(string relationshipType, string contentType)
            {
                RelationshipType = relationshipType;
                ContentType = contentType;
            }
        }

        private async Task<(string[] embeddableContentTypes, IEnumerable<string> relationshipTypes)>
            GetEmbeddableContentTypesAndRelationshipTypes(IGraphSyncContext context)
        {
            string[] embeddableContentTypes = GetEmbeddableContentTypes(context).ToArray();

            IEnumerable<string> relationshipTypes = await Task.WhenAll(
                embeddableContentTypes.Select(async ct => await RelationshipType(ct)));

            return (embeddableContentTypes, relationshipTypes);
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

                    //todo: probably belongs in deletegraphsyncer DeleteEmbeddedAllowed
                    var allDeleteIncomingRelationshipsProperties = new HashSet<KeyValuePair<string, object>>();

                    if (context.DeleteIncomingRelationshipsProperties != null)
                    {
                        allDeleteIncomingRelationshipsProperties.UnionWith(context.DeleteIncomingRelationshipsProperties);
                    }

                    allDeleteIncomingRelationshipsProperties.UnionWith(TwoWayRelationshipProperties);

                    IAllowSyncResult embeddedAllowSyncResult = await deleteGraphSyncer.DeleteAllowed(
                        contentItem,
                        context.ContentItemVersion,
                        context.DeleteOperation,
                        allDeleteIncomingRelationshipsProperties,
                        context);

                    allowSyncResult.AddRelated(embeddedAllowSyncResult);
                }
            }
        }

        //todo: best place for this to live?
        private static IEnumerable<KeyValuePair<string, object>> TwoWayRelationshipProperties { get; } =
            new Dictionary<string, object> { { NodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName, true } };

        public async Task DeleteComponents(JArray? contentItems, IGraphDeleteContext context)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            IEnumerable<string> embeddableContentTypes = GetEmbeddableContentTypes(context);

            var embeddedContentItemsByType = embeddedContentItems
                .GroupBy(ci => ci.ContentType)
                .Where(g => embeddableContentTypes.Contains(g.Key));

            foreach (var embeddedContentItemsOfType in embeddedContentItemsByType)
            {
                foreach (ContentItem contentItem in embeddedContentItemsOfType)
                {
                    await context.DeleteGraphSyncer.DeleteEmbedded(contentItem);
                }
            }
        }

        //todo: change sigs to accept ContentItem[]??
        public async Task<ContentItem[]> MutateOnClone(JArray? contentItems, ICloneContext context)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            foreach (var contentItem in embeddedContentItems)
            {
                ICloneGraphSync cloneGraphSync = _serviceProvider.GetRequiredService<ICloneGraphSync>();

                await cloneGraphSync.MutateOnClone(contentItem, context.ContentManager, context);
            }

            return embeddedContentItems;
        }

        private void DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(IGraphMergeContext context)
        {
            if (_removingRelationships?.Any() != true)
            {
                // nothing to do here, not removing any relationships
                return;
            }

            var deleteRelationshipsCommand = _serviceProvider.GetRequiredService<IDeleteRelationshipsCommand>();

            deleteRelationshipsCommand.DeleteDestinationNodes = true;
            deleteRelationshipsCommand.SourceNodeLabels = context.ReplaceRelationshipsCommand.SourceNodeLabels;
            deleteRelationshipsCommand.SourceIdPropertyName = context.ReplaceRelationshipsCommand.SourceIdPropertyName;
            deleteRelationshipsCommand.SourceIdPropertyValue =
                context.ReplaceRelationshipsCommand.SourceIdPropertyValue;

            deleteRelationshipsCommand.AddRelationshipsTo(_removingRelationships);

            context.ExtraCommands.Add(deleteRelationshipsCommand);
        }

        private List<CommandRelationship> GetRemovingRelationships(
            IEnumerable<CommandRelationship> existing,
            IEnumerable<CommandRelationship> required,
            ISyncNameProvider syncNameProvider)
        {
            List<CommandRelationship> removingRelationships = new List<CommandRelationship>();

            var distinctExistingRelationshipsTypes = existing
                .Select(r => (r.RelationshipType,
                    DestinationNodeLabel: syncNameProvider.GetContentTypeFromNodeLabels(r.DestinationNodeLabels)))
                .Distinct();

            foreach (var distinctExistingRelationshipsType in distinctExistingRelationshipsTypes)
            {
                var (existingIdPropertyValues, existingRelationshipsOfType)
                    = GetIdPropertyValuesAndRelationshipsOfType(
                        existing, distinctExistingRelationshipsType, syncNameProvider);
                if (!existingIdPropertyValues.Any())
                    continue;

                var (requiredIdPropertyValues, _) = GetIdPropertyValuesAndRelationshipsOfType(
                    required, distinctExistingRelationshipsType, syncNameProvider);

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
                ISyncNameProvider syncNameProvider)
        {
            var relationshipsOfType = relationships
                .Where(r =>
                    syncNameProvider.GetContentTypeFromNodeLabels(r.DestinationNodeLabels) ==
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

                var embeddedContentNameProvider = _serviceProvider.GetRequiredService<ISyncNameProvider>();
                embeddedContentNameProvider.ContentType = embeddedContentItem.ContentType;

                // check expected relationship is in graph
                string expectedRelationshipType = await RelationshipType(embeddedContentItem.ContentType);

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.TryGetValue(expectedRelationshipType, out int currentCount);
                context.ExpectedRelationshipCounts[expectedRelationshipType] = ++currentCount;

                // we've already validated the destination node, so we can assume the id property is there
                object destinationId = embeddedContentNameProvider.GetIdPropertyValue(
                    embeddedContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string embeddedContentIdPropertyName = embeddedContentNameProvider.IdPropertyName(embeddedContentItem.ContentType);

                var expectedRelationshipProperties = await GetRelationshipProperties(
                    embeddedContentItem, relationshipOrdinal, embeddedContentNameProvider);
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

        protected abstract IEnumerable<string> GetEmbeddableContentTypes(IGraphSyncContext context);

        //todo: need to separate SyncNameProvider into stateful and stateless
        protected virtual async Task<string> RelationshipType(string contentType)
        {
            return await _statelessSyncNameProvider.RelationshipTypeDefault(contentType);
        }

        protected virtual Task<string?> TwoWayIncomingRelationshipType(string contentType)
        {
            return Task.FromResult(default(string));
        }

        protected virtual Task<Dictionary<string, object>?> GetRelationshipProperties(
            ContentItem contentItem,
            int ordinal,
            ISyncNameProvider syncNameProvider)
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

        public async Task AddRelationship(JArray? contentItems, IDescribeRelationshipsContext context)
        {
            if (contentItems == null)
            {
                return;
            }

            var convertedItems = ConvertToContentItems(contentItems);

            foreach (var embeddedContentItem in convertedItems)
            {
                var embeddedContentGraphSyncHelper = _serviceProvider.GetRequiredService<ISyncNameProvider>();
                embeddedContentGraphSyncHelper.ContentType = embeddedContentItem.ContentType;

                string relationshipType = await RelationshipType(embeddedContentItem.ContentType);

                context.AvailableRelationships.Add(new ContentItemRelationship(await context.SyncNameProvider.NodeLabels(context.ContentItem.ContentType), relationshipType, await context.SyncNameProvider.NodeLabels(embeddedContentItem.ContentType)));

                var describeRelationshipService = _serviceProvider.GetRequiredService<IDescribeContentItemHelper>();

                var childContext = new DescribeRelationshipsContext(context.SourceNodeIdPropertyName, context.SourceNodeId, context.SourceNodeLabels, embeddedContentItem, context.SyncNameProvider, context.ContentManager, context.ContentItemVersion, context, context.ServiceProvider, context.RootContentItem);
                childContext.SetContentField(embeddedContentItem.Content);
                await describeRelationshipService.BuildRelationships(embeddedContentItem, childContext);
            }
        }
    }
}
