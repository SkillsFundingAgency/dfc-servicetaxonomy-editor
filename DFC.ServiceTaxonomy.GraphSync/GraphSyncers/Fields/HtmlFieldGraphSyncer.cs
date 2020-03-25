using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class HtmlFieldGraphSyncer : FieldGraphSyncer, IContentFieldGraphSyncer
    {
        public string FieldTypeName => "HtmlField";

        private const string ContentKey = "Html";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            JValue? value = (JValue?)contentItemField[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            mergeNodeCommand.Properties.Add(await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name), value.As<string>());
        }

        public async Task<bool> VerifySyncComponent(
            JObject contentItemField,
            ContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);

            return StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                sourceNode);

            // //todo: helper for this?
            // string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);
            // sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);
            //
            // JValue? contentItemFieldValue = (JValue?)contentItemField?[ContentKey];
            // if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            // {
            //     return nodePropertyValue == null;
            // }
            //
            // if (nodePropertyValue == null)
            //     return false;
            //
            // return contentItemFieldValue.As<string>() == (string)nodePropertyValue;
        }
    }
}
