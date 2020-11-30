using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
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
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "ContentPickerField";

        private const string ContentItemIdsKey = "ContentItemIds";
        //todo: move into hidden ## section?
        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        private readonly IServiceProvider _serviceProvider;

        public const string ContentPickerRelationshipPropertyName = "contentPicker";

        public static IEnumerable<KeyValuePair<string, object>> ContentPickerRelationshipProperties { get; } =
            new Dictionary<string, object> { { ContentPickerRelationshipPropertyName, true } };

        public ContentPickerFieldGraphSyncer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            await AddSyncComponents(context, (JArray?)contentItemField[ContentItemIdsKey]);
        }

        private async Task AddSyncComponents(IGraphMergeContext context, JArray? contentItemIdsJArray = null)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                context.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

            //todo: use pickedContentSyncNameProvider in RelationshipTypeContentPicker?
            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, context.SyncNameProvider);

            //todo: support multiple pickable content types
            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];

            ISyncNameProvider pickedContentSyncNameProvider = _serviceProvider.GetSyncNameProvider(pickedContentType);

            IEnumerable<string> destNodeLabels = await pickedContentSyncNameProvider.NodeLabels();

            if (contentItemIdsJArray?.HasValues != true)
            {
                context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
                    relationshipType,
                    destNodeLabels);
                return;
            }

            ContentItem[] foundDestinationContentItems = await GetContentItemsFromIds(
                contentItemIdsJArray,
                context.ContentManager,
                context.ContentItemVersion);

            if (context.ContentItemVersion.GraphReplicaSetName == GraphReplicaSetNames.Preview
                && foundDestinationContentItems.Count() != contentItemIdsJArray.Count)
            {
                throw new GraphSyncException(
                    $"Missing picked content items. Looked for {string.Join(",", contentItemIdsJArray.Values<string?>())}. Found {string.Join(",", foundDestinationContentItems.Select(i => i.ContentItemId))}. Current merge node command: {context.MergeNodeCommand}.");
            }

            // if we're syncing to the published graph, some items may be draft only,
            // so it's valid to have less found content items than are picked
            //todo: we could also check when publishing and take into account how many we expect not to find as they are draft only

            IEnumerable<object> foundDestinationNodeIds =
                foundDestinationContentItems.Select(ci => GetNodeId(ci!, pickedContentSyncNameProvider, context.ContentItemVersion));

            long ordinal = 0;

            foreach (var item in foundDestinationNodeIds)
            {
                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                ContentPickerRelationshipProperties.Concat(new[] { new KeyValuePair<string, object>("Ordinal", ordinal++) }),
                destNodeLabels,
                pickedContentSyncNameProvider.IdPropertyName(),
                item);
            }
        }

        public async Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            await AddSyncComponents(context);
        }

        public async Task AddRelationship(JObject contentItemField, IDescribeRelationshipsContext parentContext)
        {
            var describeContentItemHelper = _serviceProvider.GetRequiredService<IDescribeContentItemHelper>();

            ContentPickerFieldSettings contentPickerFieldSettings =
                parentContext.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

            JArray? contentItemIdsJArray = (JArray?)contentItemField[ContentItemIdsKey];

            if (contentItemIdsJArray?.HasValues == true)
            {
                string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, parentContext.SyncNameProvider);
                var sourceNodeLabels = await parentContext.SyncNameProvider.NodeLabels(parentContext.ContentItem.ContentType);
                string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];
                IEnumerable<string> destNodeLabels = await parentContext.SyncNameProvider.NodeLabels(pickedContentType);
                parentContext.AvailableRelationships.Add(new ContentItemRelationship(sourceNodeLabels, relationshipType, destNodeLabels));

                //todo: do we need each child, or can we just have a hashset of types
                foreach (var nestedItem in contentItemIdsJArray)
                {
                    await describeContentItemHelper.BuildRelationships(nestedItem.Value<string>(), parentContext);
                }
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

            ContentItem[] destinationContentItems = await GetContentItemsFromIds(
                contentItemIds,
                context.ContentManager,
                context.ContentItemVersion);

            //todo: separate check for missing items, before check relationships
            //todo: move into helper?

            //todo: have to allow for published picker referencing draft item
            if (destinationContentItems.Count() != actualRelationships.Length)
            {
                return (false, $"expecting {destinationContentItems.Count()} relationships of type {relationshipType} in graph, but found {actualRelationships.Length}");
            }

            long ordinal = 0;

            foreach (ContentItem destinationContentItem in destinationContentItems)
            {
                //todo: should logically be called using destination ContentType, but it makes no difference atm
                object destinationId = context.SyncNameProvider.GetNodeIdPropertyValue(
                    destinationContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string destinationIdPropertyName =
                    context.SyncNameProvider.IdPropertyName(destinationContentItem.ContentType);

                var expectedRelationshipProperties =
                    ContentPickerRelationshipProperties.Concat(
                        new[] {new KeyValuePair<string, object>("Ordinal", ordinal++)});

                //todo: we might want to check that all the supplied relationship properties are there,
                // whilst not failing validation if other properties are present?
                (bool validated, string failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithOutgoingRelationships,
                    relationshipType,
                    destinationIdPropertyName,
                    destinationId,
                    expectedRelationshipProperties);

                if (!validated)
                    return (false, failureReason);

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.IncreaseCount(relationshipType);
            }

            return (true, "");
        }

        private async Task<ContentItem[]> GetContentItemsFromIds(
            JArray contentItemIds,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion)
        {
            // GetAsync should be returning ContentItem? as it can be null

            ContentItem?[] contentItems = await Task.WhenAll(contentItemIds
                .Select(idJToken => idJToken.ToObject<string?>())
                .Select(async id => await contentItemVersion.GetContentItem(contentManager, id!)));

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

        private object GetNodeId(
            ContentItem pickedContentItem,
            ISyncNameProvider syncNameProvider,
            IContentItemVersion contentItemVersion)
        {
            //todo: add GetNodeId support to TaxonomyFieldGraphSyncer

            return syncNameProvider.GetNodeIdPropertyValue(
                pickedContentItem.Content[nameof(GraphSyncPart)], contentItemVersion);
        }
    }
}
