using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "ContentPickerField";

        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        private readonly ILogger<ContentPickerFieldGraphSyncer> _logger;

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

            JArray? contentItemIdsJArray = (JArray?)contentItemField["ContentItemIds"];
            if (contentItemIdsJArray == null || !contentItemIdsJArray.HasValues)
                return; //todo:

            IEnumerable<string> contentItemIds = contentItemIdsJArray.Select(jtoken => jtoken.ToString());

            // GetAsync should be returning ContentItem? as it can be null
            //todo: think just getting the latest should be fine, we only use them for the id, which should be the same whether draft or published
            // but if saving a published item which has references to draft content items, then draft item won't be in the published graph
            // need to decide between...
            // * don't create relationships from a pub to draft items, then when a draft item is published, query the draft graph for incoming relationships, and create those incoming relationships on the newly published item in the published graph
            // * create placeholder node in the published database when a draft version is saved and there's no published version, then filter our relationships to placeholder nodes in content api etc.
            IEnumerable<Task<ContentItem>> destinationContentItemsTasks =
                contentItemIds.Select(async contentItemId =>    //todo: add method to context?
                    await context.ContentItemVersion.GetContentItem(context.ContentManager, contentItemId));

            ContentItem?[] destinationContentItems = await Task.WhenAll(destinationContentItemsTasks);

            IEnumerable<ContentItem?> foundDestinationContentItems =
                destinationContentItems.Where(ci => ci != null);

            if (foundDestinationContentItems.Count() != contentItemIds.Count())
                throw new GraphSyncException($"Missing picked content items. Looked for {string.Join(",", contentItemIds)}. Found {string.Join(",", foundDestinationContentItems)}. Current merge node command: {context.MergeNodeCommand}.");

            // warning: we should logically be passing an IGraphSyncHelper with its ContentType set to pickedContentType
            // however, GetIdPropertyValue() doesn't use the set ContentType, so this works
            IEnumerable<object> foundDestinationNodeIds =
                foundDestinationContentItems.Select(ci => GetNodeId(ci!, context));

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                null,
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

            var contentItemIds = (JArray)contentItemField["ContentItemIds"]!;
            if (contentItemIds.Count != actualRelationships.Length)
            {
                return (false, $"expecting {contentItemIds.Count} relationships of type {relationshipType} in graph, but found {actualRelationships.Length}");
            }

            foreach (JToken item in contentItemIds)
            {
                string contentItemId = (string)item!;

                ContentItem destinationContentItem = await context.ContentItemVersion.GetContentItem(context.ContentManager, contentItemId);

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
