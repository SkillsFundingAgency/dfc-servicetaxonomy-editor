using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
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

        private static readonly IReadOnlyDictionary<string, object> _contentPickerRelationshipProperties =
            new Dictionary<string, object> {{ContentPickerRelationshipPropertyName, true}};

        public ContentPickerFieldGraphSyncer(
            ILogger<ContentPickerFieldGraphSyncer> logger)
        {
            _logger = logger;
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

            //todo: think just getting the latest should be fine, we only use them for the id, which should be the same whether draft or published
            // but if saving a published item which has references to draft content items, then draft item won't be in the published graph
            // need to decide between...
            // * don't create relationships from a pub to draft items, then when a draft item is published, query the draft graph for incoming relationships, and create those incoming relationships on the newly published item in the published graph
            // other things, e.g. non oc controlled things might have incoming relationships. check all contentpicker content part definitions, that can pick the content. then check all the items in all parts? or query incoming on draft and use uri to pick from content items/create index?
            // create oc table of all draft items with referencing published items? or store in graph?
            // create ghosted items not connected to published content? in own graph?
            // * create placeholder node in the published database when a draft version is saved and there's no published version, then filter our relationships to placeholder nodes in content api etc.

            ContentItem[] foundDestinationContentItems = await GetLatestContentItemsFromIds(contentItemIdsJArray, context);

            if (foundDestinationContentItems.Count() != contentItemIdsJArray.Count)
                throw new GraphSyncException(
                    $"Missing picked content items. Looked for {string.Join(",", contentItemIdsJArray.Values<string?>())}. Found {string.Join(",", foundDestinationContentItems.Select(i => i.ContentItemId))}. Current merge node command: {context.MergeNodeCommand}.");

            //todo: extract
            if (context.ContentItemVersion.GraphReplicaSetName == GraphReplicaSetNames.Published)
            {
                //todo: better to add property to picker relationships in preview graph that aren't in published graph
                // then can query them and recreate them on item publish (merge node)
                // rather than storing the triplets separately

                // split foundDestinationContentItems into those that are published and all the others
                // all others create relationships as normal

                //just where?
                ILookup<bool, ContentItem> arePublishedContentItems = foundDestinationContentItems
                    .ToLookup(i => i!.Published);

                foundDestinationContentItems = arePublishedContentItems[true].ToArray();

                // draft only create
                // if republish, don't want to create new ghost triplets, but want them to be unique and not relate to each other
                // store source+ghost id in source and dest to make unique and match on both
            }

            // warning: we should logically be passing an IGraphSyncHelper with its ContentType set to pickedContentType
            // however, GetIdPropertyValue() doesn't use the set ContentType, so this works
            IEnumerable<object> foundDestinationNodeIds =
                foundDestinationContentItems.Select(ci => GetNodeId(ci!, context));

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                _contentPickerRelationshipProperties,
                destNodeLabels,
                context.GraphSyncHelper.IdPropertyName(pickedContentType),
                foundDestinationNodeIds.ToArray());
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
            // move into helper??? prob not

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

            //todo: validate that _contentPickerRelationshipProperties are there

            foreach (ContentItem destinationContentItem in destinationContentItems)
            {
                //todo: should logically be called using destination ContentType, but it makes no difference atm
                object destinationId = context.GraphSyncHelper.GetIdPropertyValue(
                    destinationContentItem.Content.GraphSyncPart, context.ContentItemVersion);

                string destinationIdPropertyName =
                    context.GraphSyncHelper.IdPropertyName(destinationContentItem.ContentType);

                (bool validated, string failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithOutgoingRelationships,
                    relationshipType,
                    destinationIdPropertyName,
                    destinationId);

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
                //.Select(async id => await context.ContentItemVersion.GetContentItem(context.ContentManager, id!)));
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
