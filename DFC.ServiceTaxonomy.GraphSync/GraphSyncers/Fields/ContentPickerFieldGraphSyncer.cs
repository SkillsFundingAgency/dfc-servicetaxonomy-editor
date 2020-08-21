using System.Linq;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "ContentPickerField";

        private const string ContentItemIdsKey = "ContentItemIds";
        //todo: move into hidden ## section?
        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        private readonly ILogger<ContentPickerFieldGraphSyncer> _logger;

        public const string ContentPickerRelationshipPropertyName = "contentPicker";

        public static IEnumerable<KeyValuePair<string, object>> ContentPickerRelationshipProperties { get; } =
            new Dictionary<string, object> { { ContentPickerRelationshipPropertyName, true } };

        public ContentPickerFieldGraphSyncer(
            ILogger<ContentPickerFieldGraphSyncer> logger)
        {
            _logger = logger;
        }

        public async Task AddRelationship(IDescribeRelationshipsContext parentContext)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
               parentContext.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, parentContext.GraphSyncHelper);

            //var describeRelationshipsContext = new DescribeRelationshipsContext(parentContext.ContentItem, parentContext.GraphSyncHelper, parentContext.ContentManager, parentContext.ContentItemVersion, parentContext, parentContext.ServiceProvider) { AvailableRelationships = new List<string>() { relationshipType } };

            parentContext.AvailableRelationships.Add(new ContentItemRelationship(parentContext.ContentItem.ContentType, relationshipType, await parentContext.GraphSyncHelper.NodeLabels(contentPickerFieldSettings.DisplayedContentTypes.FirstOrDefault())));
        }

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                context.ContentPartFieldDefinition!.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, context.GraphSyncHelper);

            //todo: support multiple pickable content types
            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];
            IEnumerable<string> destNodeLabels = await context.GraphSyncHelper.NodeLabels(pickedContentType);

            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            JArray? contentItemIdsJArray = (JArray?)contentItemField[ContentItemIdsKey];
            if (contentItemIdsJArray?.HasValues != true)
            {
                context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
                    relationshipType,
                    null,
                    destNodeLabels,
                    context.GraphSyncHelper.IdPropertyName(pickedContentType));
                return;
            }

            ContentItem[] foundDestinationContentItems = await GetLatestContentItemsFromIds(contentItemIdsJArray, context);

            if (foundDestinationContentItems.Count() != contentItemIdsJArray.Count)
                throw new GraphSyncException(
                    $"Missing picked content items. Looked for {string.Join(",", contentItemIdsJArray.Values<string?>())}. Found {string.Join(",", foundDestinationContentItems.Select(i => i.ContentItemId))}. Current merge node command: {context.MergeNodeCommand}.");

            if (context.ContentItemVersion.GraphReplicaSetName == GraphReplicaSetNames.Published)
            {
                foundDestinationContentItems = foundDestinationContentItems
                    .Where(i => i.Published)
                    .ToArray();
            }

            // warning: we should logically be passing an IGraphSyncHelper with its ContentType set to pickedContentType
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
                context.GraphSyncHelper.IdPropertyName(pickedContentType),
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

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, context.GraphSyncHelper);

            IOutgoingRelationship[] actualRelationships = context.NodeWithOutgoingRelationships.OutgoingRelationships
                .Where(r => r.Relationship.Type == relationshipType)
                .ToArray();

            var contentItemIds = (JArray)contentItemField[ContentItemIdsKey]!;

            ContentItem[] destinationContentItems = await GetLatestContentItemsFromIds(contentItemIds, context);

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
                object destinationId = context.GraphSyncHelper.GetIdPropertyValue(
                    destinationContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string destinationIdPropertyName =
                    context.GraphSyncHelper.IdPropertyName(destinationContentItem.ContentType);

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

        private async Task<ContentItem[]> GetLatestContentItemsFromIds(JArray contentItemIds, IGraphOperationContext context)
        {
            // GetAsync should be returning ContentItem? as it can be null

            ContentItem?[] contentItems = await Task.WhenAll(contentItemIds
                .Select(idJToken => idJToken.ToObject<string?>())
                .Select(async id => await context.ContentManager.GetAsync(id, VersionOptions.Latest)));

#pragma warning disable S1905
            return contentItems
                .Where(ci => ci != null)
                .Cast<ContentItem>()
                .ToArray();
#pragma warning restore S1905
        }

        private async Task<string> RelationshipTypeContentPicker(
            ContentPickerFieldSettings contentPickerFieldSettings,
            IGraphSyncHelper graphSyncHelper)
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
                relationshipType = await graphSyncHelper!.RelationshipTypeDefault(pickedContentType);

            return relationshipType;
        }

        private object GetNodeId(ContentItem pickedContentItem, IGraphMergeContext context)
        {
            return context.GraphSyncHelper.GetIdPropertyValue(
                pickedContentItem.Content[nameof(GraphSyncPart)], context.ContentItemVersion);
        }
    }
}
