using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Commands;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.DataSync.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Helpers
{
    public abstract class CosmosDbEmbeddedContentItemsDataSyncer : IEmbeddedContentItemsDataSyncer
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly ISyncNameProvider _statelessSyncNameProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;
        private List<CommandRelationship>? _removingRelationships;

        protected CosmosDbEmbeddedContentItemsDataSyncer(
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
            IDataMergeContext context,
            IAllowSync allowSync)
        {
            _logger.LogDebug("Do embedded items allow sync?");

            List<CommandRelationship> requiredRelationships = await GetRequiredRelationshipsAndOptionallySync(contentItems, context, allowSync);

            INodeAndOutRelationshipsAndTheirInRelationships? existing = (await context.DataSyncReplicaSet.Run(
                    new CosmosDbNodeAndOutRelationshipsAndTheirInRelationshipsQuery(
                        context.ReplaceRelationshipsCommand.SourceNodeLabels,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyName!,
                        context.ReplaceRelationshipsCommand.SourceIdPropertyValue!)))
                .FirstOrDefault();

            if (existing?.OutgoingRelationships?.Any() != true)
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
                            CosmosDbNodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName));

                    allowSync.AddSyncBlockers(
                        nonTwoWayIncomingRelationshipsToEmbeddedItems.Select(r =>
                        {
                            string contentType =
                                context.SyncNameProvider.GetContentTypeFromNodeLabels(r.DestinationNode.Labels);
                            return new SyncBlocker(
                                contentType,
                                r.DestinationNode.Properties[context.SyncNameProvider.IdPropertyName(contentType)],
                                (string?)r.DestinationNode.Properties[TitlePartDataSyncer.NodeTitlePropertyName]);
                        }));
                }
            }
        }

        private IMergeDataSyncer GetNewMergeDataSyncer()
        {
            _logger.LogDebug("Getting new IMergeDataSyncer.");

            return _serviceProvider.GetRequiredService<IMergeDataSyncer>();
        }

        private async Task<List<CommandRelationship>> GetRequiredRelationshipsAndOptionallySync(
            JArray? contentItems,
            IDataMergeContext context,
            IAllowSync? allowSync = null)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            List<CommandRelationship> requiredRelationships = new List<CommandRelationship>();

            int relationshipOrdinal = 0;
            foreach (ContentItem contentItem in embeddedContentItems)
            {
                IMergeDataSyncer? mergDataSyncer;

                if (allowSync == null)
                {
                    // we're actually syncing, not checking if it's allowed
                    mergDataSyncer = await context.MergeDataSyncer.SyncEmbedded(contentItem);
                }
                else
                {
                    mergDataSyncer = GetNewMergeDataSyncer();

                    IAllowSync embeddedAllowSync = await mergDataSyncer.SyncAllowed(context.DataSyncReplicaSet, contentItem, context.ContentManager, context);
                    allowSync.AddRelated(embeddedAllowSync);
                    if (embeddedAllowSync.Result != AllowSyncResult.Allowed)
                        continue;
                }

                //todo: check embedded items with no graphsyncpart attached
                if (mergDataSyncer == null)
                    continue;

                IMergeNodeCommand containedContentMergeNodeCommand = mergDataSyncer.MergeNodeCommand;
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

        public async Task AddSyncComponents(JArray? contentItems, IDataMergeContext context)
        {
            List<CommandRelationship> requiredRelationships = await GetRequiredRelationshipsAndOptionallySync(contentItems, context);

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(requiredRelationships);

            DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(context);
        }

        public async Task AllowSyncDetaching(IDataMergeContext context, IAllowSync allowSync)
        {
            //todo: factor out common code

            INodeAndOutRelationshipsAndTheirInRelationships? existing = (await context.DataSyncReplicaSet.Run(
                    new CosmosDbNodeAndOutRelationshipsAndTheirInRelationshipsQuery(
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
                            CosmosDbNodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName));

                        allowSync.AddSyncBlockers(
                            nonTwoWayIncomingRelationshipsToEmbeddedItems.Select(r =>
                            {
                                string contentType =
                                    context.SyncNameProvider.GetContentTypeFromNodeLabels(r.DestinationNode.Labels);
                                return new SyncBlocker(
                                    contentType,
                                    r.DestinationNode.Properties[context.SyncNameProvider.IdPropertyName(contentType)],
                                    (string?)r.DestinationNode.Properties[TitlePartDataSyncer.NodeTitlePropertyName]);
                            }));
                }
            }
        }

        public async Task AddSyncComponentsDetaching(IDataMergeContext context)
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
            GetEmbeddableContentTypesAndRelationshipTypes(IDataSyncContext context)
        {
            string[] embeddableContentTypes = GetEmbeddableContentTypes(context).ToArray();

            IEnumerable<string> relationshipTypes = await Task.WhenAll(
                embeddableContentTypes.Select(async ct => await RelationshipType(ct)));

            return (embeddableContentTypes, relationshipTypes);
        }

        public async Task AllowDelete(
            JArray? contentItems,
            IDataDeleteContext context,
            IAllowSync allowSync)
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
                    IDeleteDataSyncer deleteDataSyncer = _serviceProvider.GetRequiredService<IDeleteDataSyncer>();

                    //todo: probably belongs in deletedatasyncer DeleteEmbeddedAllowed
                    var allDeleteIncomingRelationshipsProperties = new HashSet<KeyValuePair<string, object>>();

                    if (context.DeleteIncomingRelationshipsProperties != null)
                    {
                        allDeleteIncomingRelationshipsProperties.UnionWith(context.DeleteIncomingRelationshipsProperties);
                    }

                    allDeleteIncomingRelationshipsProperties.UnionWith(TwoWayRelationshipProperties);

                    IAllowSync embeddedAllowSync = await deleteDataSyncer.DeleteAllowed(
                        contentItem,
                        context.ContentItemVersion,
                        context.SyncOperation,
                        allDeleteIncomingRelationshipsProperties,
                        context);

                    allowSync.AddRelated(embeddedAllowSync);
                }
            }
        }

        //todo: best place for this to live?
        private static IEnumerable<KeyValuePair<string, object>> TwoWayRelationshipProperties { get; } =
            new Dictionary<string, object> { { CosmosDbNodeWithOutgoingRelationshipsCommand.TwoWayRelationshipPropertyName, true } };

        public async Task DeleteComponents(JArray? contentItems, IDataDeleteContext context)
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
                    await context.DeleteDataSyncer.DeleteEmbedded(contentItem);
                }
            }
        }

        //todo: change sigs to accept ContentItem[]??
        public async Task<ContentItem[]> MutateOnClone(JArray? contentItems, ICloneContext context)
        {
            ContentItem[] embeddedContentItems = ConvertToContentItems(contentItems);

            foreach (var contentItem in embeddedContentItems)
            {
                ICloneDataSync cloneDataSync = _serviceProvider.GetRequiredService<ICloneDataSync>();

                await cloneDataSync.MutateOnClone(contentItem, context.ContentManager, context);
            }

            return embeddedContentItems;
        }

        private void DeleteRelationshipsOfNonEmbeddedButAllowedContentTypes(IDataMergeContext context)
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
                // embedded item doesn't have a graph sync part, so doesn't get synced
                if (!_contentTypes.ContainsKey(embeddedContentItem.ContentType))
                    continue;

                ContentTypeDefinition embeddedContentTypeDefinition = _contentTypes[embeddedContentItem.ContentType];

                (bool validated, string failureReason) =
                    await context.ValidateAndRepairData.ValidateContentItem(
                        embeddedContentItem, embeddedContentTypeDefinition, context.ContentItemVersion);

                if (!validated)
                    return (false, $"contained item failed validation: {failureReason}");

                var embeddedContentNameProvider = _serviceProvider.GetSyncNameProvider(embeddedContentItem.ContentType);

                // check expected relationship is in data sync
                string expectedRelationshipType = await RelationshipType(embeddedContentItem.ContentType);

                // keep a count of how many relationships of a type we expect to be in the data sync
                context.ExpectedRelationshipCounts.TryGetValue(expectedRelationshipType, out int currentCount);
                context.ExpectedRelationshipCounts[expectedRelationshipType] = ++currentCount;

                // we've already validated the destination node, so we can assume the id property is there
                object destinationId = embeddedContentNameProvider.GetNodeIdPropertyValue(
                    embeddedContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string embeddedContentIdPropertyName = embeddedContentNameProvider.IdPropertyName(embeddedContentItem.ContentType);

                var expectedRelationshipProperties = await GetRelationshipProperties(
                    embeddedContentItem, relationshipOrdinal, embeddedContentNameProvider);
                ++relationshipOrdinal;

                (validated, failureReason) = context.DataSyncValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithRelationships,
                    expectedRelationshipType,
                    embeddedContentIdPropertyName,
                    destinationId,
                    expectedRelationshipProperties);

                if (!validated)
                    return (false, failureReason);

                string? twoWayRelationshipType = await TwoWayIncomingRelationshipType(embeddedContentItem.ContentType);
                if (twoWayRelationshipType != null)
                {
                    (validated, failureReason) = context.DataSyncValidationHelper.ValidateIncomingRelationship(
                        context.NodeWithRelationships,
                        twoWayRelationshipType,
                        embeddedContentIdPropertyName,
                        destinationId,
                        TwoWayRelationshipProperties);

                    if (!validated)
                        return (false, failureReason);
                }
            }

            return (true, "");
        }

        protected abstract IEnumerable<string> GetEmbeddableContentTypes(IDataSyncContext context);

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
                throw new DataSyncException("Embedded content container has missing array.");
            }

            ContentItem[]? embeddedContentItems = contentItems.ToObject<ContentItem[]>();
            if (embeddedContentItems == null)
                throw new DataSyncException("Embedded content container does not contain ContentItems.");
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
                //todo: pass to BuildRelationships??

                string relationshipType = await RelationshipType(embeddedContentItem.ContentType);

                context.AvailableRelationships.Add(new ContentItemRelationship(
                    await context.SyncNameProvider.NodeLabels(context.ContentItem.ContentType),
                    relationshipType,
                    await context.SyncNameProvider.NodeLabels(embeddedContentItem.ContentType)));

                var describeRelationshipService = _serviceProvider.GetRequiredService<IDescribeContentItemHelper>();

                //todo: version that accepts existing context, but why have all the child contexts with the same properties?
                await describeRelationshipService.BuildRelationships(
                    embeddedContentItem,
                    context.SourceNodeIdPropertyName,
                    context.SourceNodeId,
                    context.SourceNodeLabels,
                    context.SyncNameProvider,
                    context.ContentManager,
                    context.ContentItemVersion,
                    context,
                    context.ServiceProvider);
            }
        }
    }
}
