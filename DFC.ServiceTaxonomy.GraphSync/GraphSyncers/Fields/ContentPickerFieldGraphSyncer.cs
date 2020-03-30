using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "ContentPickerField";

        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        private readonly IContentManager _contentManager;
        private readonly ILogger<ContentPickerFieldGraphSyncer> _logger;

        public ContentPickerFieldGraphSyncer(
            IContentManager contentManager,
            ILogger<ContentPickerFieldGraphSyncer> logger)
        {
            _contentManager = contentManager;
            _logger = logger;
        }

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                contentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, graphSyncHelper);

            //todo: support multiple pickable content types
            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];
            IEnumerable<string> destNodeLabels = await graphSyncHelper.NodeLabels(pickedContentType);

            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            JArray? contentItemIdsJArray = (JArray?)contentItemField["ContentItemIds"];
            if (contentItemIdsJArray == null || !contentItemIdsJArray.HasValues)
                return; //todo:

            IEnumerable<string> contentItemIds = contentItemIdsJArray.Select(jtoken => jtoken.ToString());

            // cast is not redundant, GetAsync should be returning ContentItem?
            #pragma warning disable S1905
            IEnumerable<Task<ContentItem?>> destinationContentItemsTasks =
                (IEnumerable<Task<ContentItem?>>)contentItemIds.Select(async contentItemId =>
                    await _contentManager.GetAsync(contentItemId, VersionOptions.Latest));
            #pragma warning restore S1905

            ContentItem?[] destinationContentItems = await Task.WhenAll(destinationContentItemsTasks);

            IEnumerable<ContentItem> foundDestinationContentItems =
                destinationContentItems.Where(ci => ci != null)
                    .Select(ci => ci!);

            if (foundDestinationContentItems.Count() != contentItemIds.Count())
                throw new GraphSyncException($"Missing picked content items. Looked for {string.Join(",", contentItemIds)}. Found {string.Join(",", foundDestinationContentItems)}. Current merge node command: {mergeNodeCommand}.");

            IEnumerable<string> foundDestinationNodeIds =
                foundDestinationContentItems.Select(ci => GetNodeId(ci, graphSyncHelper));

            // object[] foundDestinationNodeIds = (object[])destinationNodeIds
            //     .Where(i => i != null);
            //
            // if (foundDestinationNodeIds.Length != contentItemIds.Count)
            // {
            //     //todo: when we encounter a graph sync error during import what's the best way to handle it?
            //     throw new GraphSyncException($"{mergeNodeCommand}");
            // }

            replaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                destNodeLabels,
                graphSyncHelper!.IdPropertyName,
                foundDestinationNodeIds);
        }

        public async Task<bool> VerifySyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                contentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, graphSyncHelper);

            IRelationship[] actualRelationships = relationships
                .Where(r => r.Type == relationshipType)
                .ToArray();

            var contentItemIds = (JArray)contentItemField["ContentItemIds"]!;
            if (contentItemIds.Count != actualRelationships.Length)
            {
                _logger.LogWarning($"Sync validation failed. Expecting {actualRelationships.Length} relationships of type {relationshipType} in graph, but found {contentItemIds.Count}");
                return false;
            }

            foreach (JToken item in contentItemIds)
            {
                string contentItemId = (string)item!;

                ContentItem destinationContentItem = await _contentManager.GetAsync(contentItemId);

                string destinationId = graphSyncHelper.GetIdPropertyValue(destinationContentItem.Content.GraphSyncPart);

                INode destNode = destinationNodes.SingleOrDefault(n => (string)n.Properties[graphSyncHelper.IdPropertyName] == destinationId);
                if (destNode == null)
                {
                    _logger.LogWarning($"Sync validation failed. Destination node with user ID '{destinationId}' not found");
                    return false;
                }

                var relationship = actualRelationships.SingleOrDefault(r =>
                    r.Type == relationshipType && r.EndNodeId == destNode.Id);

                if (relationship == null)
                {
                    _logger.LogWarning($"Sync validation failed. Relationship of type {relationshipType} with end node ID {destNode.Id} not found");
                    return false;
                }
            }

            return true;
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

        private async Task<ContentItem?> GetContentItemFromContentItemId(string contentItemId)
        {
            // GetAsync should be returning ContentItem?
            return await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        }

        // private async Task<ContentItem> GetNodeIdFromContentItemIdJToken(JToken contentItemIdJToken, IGraphSyncHelper graphSyncHelper)
        // {
        //     string contentItemId = contentItemIdJToken.ToString();
        //
        //     // GetAsync should be returning ContentItem?
        //     ContentItem? contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        //     if (contentItem == null)
        //         throw new GraphSyncException($"Content item with id '{contentItemId}' not found in the database.");
        //
        //     return contentItem;
        // }

        private string GetNodeId(ContentItem pickedContentItem, IGraphSyncHelper graphSyncHelper)
        {
            return graphSyncHelper.GetIdPropertyValue(pickedContentItem.Content[nameof(GraphSyncPart)]);
        }

        // private async Task<object?> GetNodeIdFromContentItemIdJToken(JToken contentIdJToken, IGraphSyncHelper graphSyncHelper)
        // {
        //     // GetAsync should be returning ContentItem?
        //     ContentItem? contentItem =
        //         await _contentManager.GetAsync(contentIdJToken.ToString(), VersionOptions.Latest);
        //
        //     return contentItem == null ? null : GetNodeId(contentItem, graphSyncHelper);
        // }
        //
        // private object? GetNodeId(ContentItem pickedContentItem, IGraphSyncHelper graphSyncHelper)
        // {
        //     return graphSyncHelper.GetIdPropertyValue(pickedContentItem.Content[nameof(GraphSyncPart)]);
        // }
    }
}
