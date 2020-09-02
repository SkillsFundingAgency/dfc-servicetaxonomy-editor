﻿using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using System;
using OrchardCore.ContentManagement.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "ContentPickerField";

        private const string ContentItemIdsKey = "ContentItemIds";
        //todo: move into hidden ## section?
        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        private readonly IPreExistingContentItemVersion _preExistingContentItemVersion;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IServiceProvider _serviceProvider;

        public const string ContentPickerRelationshipPropertyName = "contentPicker";

        public static IEnumerable<KeyValuePair<string, object>> ContentPickerRelationshipProperties { get; } =
            new Dictionary<string, object> { { ContentPickerRelationshipPropertyName, true } };

        public ContentPickerFieldGraphSyncer(
            IPreExistingContentItemVersion preExistingContentItemVersion,
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
        {
            _preExistingContentItemVersion = preExistingContentItemVersion;
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
        }

        public async Task AddRelationship(IDescribeRelationshipsContext parentContext)
        {
            var describeContentItemHelper = _serviceProvider.GetRequiredService<IDescribeContentItemHelper>();

            ContentPickerFieldSettings contentPickerFieldSettings =
               parentContext.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            JArray? contentItemIdsJArray = (JArray?)parentContext.ContentField?[parentContext.ContentPartFieldDefinition!.Name][ContentItemIdsKey];
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (contentItemIdsJArray != null && contentItemIdsJArray.Count > 0)
            {
                string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, parentContext.SyncNameProvider);
                var sourceNodeLabels = await parentContext.SyncNameProvider.NodeLabels(parentContext.ContentItem.ContentType);
                string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];
                IEnumerable<string> destNodeLabels = await parentContext.SyncNameProvider.NodeLabels(pickedContentType);
                parentContext.AvailableRelationships.Add(new ContentItemRelationship(sourceNodeLabels, relationshipType, destNodeLabels));

                foreach (var nestedItem in contentItemIdsJArray)
                {
                    await describeContentItemHelper.BuildRelationships(nestedItem.Value<string>(), parentContext);
                }
            }
        }

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                context.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, context.SyncNameProvider);

            //todo: support multiple pickable content types
            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];
            IEnumerable<string> destNodeLabels = await context.SyncNameProvider.NodeLabels(pickedContentType);

            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            JArray? contentItemIdsJArray = (JArray?)contentItemField[ContentItemIdsKey];
            if (contentItemIdsJArray?.HasValues != true)
            {
                context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
                    relationshipType,
                    null,
                    destNodeLabels,
                    context.SyncNameProvider.IdPropertyName(pickedContentType));
                return;
            }

            ContentItem[] foundDestinationContentItems = await GetLatestContentItemsFromIds(contentItemIdsJArray, context.ContentManager);

            if (foundDestinationContentItems.Count() != contentItemIdsJArray.Count)
                throw new GraphSyncException(
                    $"Missing picked content items. Looked for {string.Join(",", contentItemIdsJArray.Values<string?>())}. Found {string.Join(",", foundDestinationContentItems.Select(i => i.ContentItemId))}. Current merge node command: {context.MergeNodeCommand}.");

            if (context.ContentItemVersion.GraphReplicaSetName == GraphReplicaSetNames.Published)
            {
                foundDestinationContentItems = foundDestinationContentItems
                    .Where(i => i.Published)
                    .ToArray();
            }

            // warning: we should logically be passing an ISyncNameProvider with its ContentType set to pickedContentType
            // however, GetIdPropertyValue() doesn't use the set ContentType, so this works
            IEnumerable<object> foundDestinationNodeIds =
                foundDestinationContentItems.Select(ci => GetNodeId(ci!, context));

            int ordinal = 0;

            foreach (var item in foundDestinationNodeIds)
            {
                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                ContentPickerRelationshipProperties.Union(new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("Ordinal", ordinal) }),
                destNodeLabels,
                context.SyncNameProvider.IdPropertyName(pickedContentType),
                item);

                ordinal++;
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            IValidateAndRepairContext context)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                context.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, context.SyncNameProvider);

            IOutgoingRelationship[] actualRelationships = context.NodeWithOutgoingRelationships.OutgoingRelationships
                .Where(r => r.Relationship.Type == relationshipType)
                .ToArray();

            var contentItemIds = (JArray)contentItemField[ContentItemIdsKey]!;

            ContentItem[] destinationContentItems = await GetLatestContentItemsFromIds(contentItemIds, context.ContentManager);

            //todo: separate check for missing items, before check relationships
            //todo: move into helper?

            //todo: equals on ContentItemVersion that checks GraphReplicaSetName
            if (context.ContentItemVersion.GraphReplicaSetName == GraphReplicaSetNames.Published)
            {
                destinationContentItems = destinationContentItems
                    .Where(i => i.Published)
                    .ToArray();
            }

            if (destinationContentItems.Count() != actualRelationships.Length)
            {
                return (false, $"expecting {destinationContentItems.Count()} relationships of type {relationshipType} in graph, but found {actualRelationships.Length}");
            }

            foreach (ContentItem destinationContentItem in destinationContentItems)
            {
                //todo: should logically be called using destination ContentType, but it makes no difference atm
                object destinationId = context.SyncNameProvider.GetIdPropertyValue(
                    destinationContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string destinationIdPropertyName =
                    context.SyncNameProvider.IdPropertyName(destinationContentItem.ContentType);

                //todo: we might want to check that all the supplied relationship properties are there,
                // whilst not failing validation if other other properties are present?
                (bool validated, string failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithOutgoingRelationships,
                    relationshipType,
                    destinationIdPropertyName,
                    destinationId,
                    ContentPickerRelationshipProperties);

                if (!validated)
                    return (false, failureReason);

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.IncreaseCount(relationshipType);
            }

            return (true, "");
        }

        private async Task<ContentItem[]> GetLatestContentItemsFromIds(JArray contentItemIds, IContentManager contentManager)
        {
            // GetAsync should be returning ContentItem? as it can be null

            ContentItem?[] contentItems = await Task.WhenAll(contentItemIds
                .Select(idJToken => idJToken.ToObject<string?>())
                .Select(async id => await contentManager.GetAsync(id, VersionOptions.Latest)));

#pragma warning disable S1905
            return contentItems
                .Where(ci => ci != null)
                .Cast<ContentItem>()
                .ToArray();
#pragma warning restore S1905
        }

        private async Task<string> RelationshipTypeContentPicker(
            ContentPickerFieldSettings contentPickerFieldSettings,
            ISyncNameProvider syncNameProvider)
        {
            //todo: handle multiple types
            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];

            string? relationshipType = null;
            if (contentPickerFieldSettings.Hint != null)
            {
                Match match = _relationshipTypeRegex.Match(contentPickerFieldSettings.Hint);
                if (match.Success)
                {
                    relationshipType = $"{match.Groups[1].Value}";
                }
            }

            if (relationshipType == null)
                relationshipType = await syncNameProvider!.RelationshipTypeDefault(pickedContentType);

            return relationshipType;
        }

        private object GetNodeId(ContentItem pickedContentItem, IGraphMergeContext context)
        {
            //Todo add GetNodeId support to TaxonomyFieldGraphSyncer
            var syncSettings = context.SyncNameProvider.GetGraphSyncPartSettings(pickedContentItem.ContentType);

            _preExistingContentItemVersion.SetContentApiBaseUrl(syncSettings.PreExistingNodeUriPrefix);

            return context.SyncNameProvider.GetIdPropertyValue(
                      pickedContentItem.Content[nameof(GraphSyncPart)], syncSettings.PreExistingNodeUriPrefix == null ? context.ContentItemVersion : _preExistingContentItemVersion );
        }
    }
}
