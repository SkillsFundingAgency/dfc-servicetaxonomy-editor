using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "ContentPickerField";

        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        private readonly IContentManager _contentManager;

        public ContentPickerFieldGraphSyncer(IContentManager contentManager)
        {
            _contentManager = contentManager;
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

            JArray? contentItemIds = (JArray?)contentItemField["ContentItemIds"];
            if (contentItemIds == null || !contentItemIds.HasValues)
                return;    //todo:

            var destIds = await Task.WhenAll(contentItemIds.Select(async relatedContentId =>
                GetSyncId(await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest), graphSyncHelper)));

            replaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                destNodeLabels,
                graphSyncHelper!.IdPropertyName,
                destIds);
        }

        public async Task<bool> VerifySyncComponent(
            JObject contentItemField,
            ContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            ContentPickerFieldSettings contentPickerFieldSettings =
                contentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

            string relationshipType = await RelationshipTypeContentPicker(contentPickerFieldSettings, graphSyncHelper);

            int relationshipCount = relationships.Count(r => string.Equals(r.Type, relationshipType, StringComparison.CurrentCultureIgnoreCase));

            var contentItemIds = (JArray)contentItemField["ContentItemIds"]!;
            if (contentItemIds.Count != relationshipCount)
            {
                return false;
            }

            foreach (JToken item in contentItemIds)
            {
                var contentItemId = (string)item!;

                var destContentItem = await _contentManager.GetAsync(contentItemId);

                //todo: rename var
                var destUri = (string)destContentItem.Content.GraphSyncPart.Text;

                var destNode = destNodes.SingleOrDefault(n => (string)n.Properties[graphSyncHelper.IdPropertyName] == destUri);

                if (destNode == null)
                {
                    return false;
                }

                var relationship = relationships.SingleOrDefault(r =>
                    string.Equals(r.Type, relationshipType, StringComparison.CurrentCultureIgnoreCase) && r.EndNodeId == destNode.Id);

                if (relationship == null)
                {
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

        private object GetSyncId(ContentItem pickedContentItem, IGraphSyncHelper graphSyncHelper)
        {
            return graphSyncHelper!.GetIdPropertyValue(pickedContentItem.Content[nameof(GraphSyncPart)]);
        }
    }
}
