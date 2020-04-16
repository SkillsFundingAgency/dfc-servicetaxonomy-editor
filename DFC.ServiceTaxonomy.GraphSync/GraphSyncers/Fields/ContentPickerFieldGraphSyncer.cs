﻿using System.Linq;
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

            // GetAsync should be returning ContentItem? as it can be null
            IEnumerable<Task<ContentItem>> destinationContentItemsTasks =
                contentItemIds.Select(async contentItemId =>
                    await _contentManager.GetAsync(contentItemId, VersionOptions.Latest));

            ContentItem?[] destinationContentItems = await Task.WhenAll(destinationContentItemsTasks);

            IEnumerable<ContentItem?> foundDestinationContentItems =
                destinationContentItems.Where(ci => ci != null);

            if (foundDestinationContentItems.Count() != contentItemIds.Count())
                throw new GraphSyncException($"Missing picked content items. Looked for {string.Join(",", contentItemIds)}. Found {string.Join(",", foundDestinationContentItems)}. Current merge node command: {mergeNodeCommand}.");

            // warning: we should logically be passing an IGraphSyncHelper with its ContentType set to pickedContentType
            // however, GetIdPropertyValue() doesn't use the set ContentType, so this works
            IEnumerable<object> foundDestinationNodeIds =
                foundDestinationContentItems.Select(ci => GetNodeId(ci!, graphSyncHelper));

            replaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                destNodeLabels,
                graphSyncHelper!.IdPropertyName(pickedContentType),
                foundDestinationNodeIds.ToArray());
        }

        public async Task<(bool verified, string failureReason)> VerifySyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
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
                return (false, $"expecting {actualRelationships.Length} relationships of type {relationshipType} in graph, but found {contentItemIds.Count}");
            }

            foreach (JToken item in contentItemIds)
            {
                string contentItemId = (string)item!;

                ContentItem destinationContentItem = await _contentManager.GetAsync(contentItemId);

                //todo: should logically be called using destination ContentType, but it makes no difference atm
                object destinationId = graphSyncHelper.GetIdPropertyValue(destinationContentItem.Content.GraphSyncPart);

                string destinationContentIdPropertyName =
                    graphSyncHelper.IdPropertyName(destinationContentItem.ContentType);
                INode destNode = destinationNodes.SingleOrDefault(n => Equals(n.Properties[destinationContentIdPropertyName], destinationId));
                if (destNode == null)
                {
                    return (false, $"destination node with user ID '{destinationId}' not found");
                }

                var relationship = actualRelationships.SingleOrDefault(r =>
                    r.Type == relationshipType && r.EndNodeId == destNode.Id);

                if (relationship == null)
                {
                    return (false, $"relationship of type {relationshipType} with end node ID {destNode.Id} not found");
                }

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.TryGetValue(relationshipType, out int currentCount);
                expectedRelationshipCounts[relationshipType] = ++currentCount;
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

        private object GetNodeId(ContentItem pickedContentItem, IGraphSyncHelper graphSyncHelper)
        {
            return graphSyncHelper.GetIdPropertyValue(pickedContentItem.Content[nameof(GraphSyncPart)]);
        }
    }
}
