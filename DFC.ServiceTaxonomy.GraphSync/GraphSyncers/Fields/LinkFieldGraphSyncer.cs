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
    public class LinkFieldGraphSyncer : FieldGraphSyncer, IContentFieldGraphSyncer
    {
        public string FieldTypeName => "LinkField";

        private const string UrlFieldKey = "Url", TextFieldKey = "Text";
        private const string LinkUrlPostfix = "_url", LinkTextPostfix = "_text";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            string basePropertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);

            JValue? value = (JValue?)contentItemField[UrlFieldKey];
            if (value != null && value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add($"{basePropertyName}{LinkUrlPostfix}", value.As<string>());

            value = (JValue?)contentItemField[TextFieldKey];
            if (value != null && value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add($"{basePropertyName}{LinkTextPostfix}", value.As<string>());
        }

        public async Task<bool> VerifySyncComponent(
            JObject contentItemField,
            ContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            string nodeBasePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);

            string nodeUrlPropertyName = $"{nodeBasePropertyName}{LinkUrlPostfix}";

            if (!StringContentPropertyMatchesNodeProperty(
                UrlFieldKey,
                contentItemField,
                nodeUrlPropertyName,
                sourceNode))
            {
                return false;
            }

            string nodeTextPropertyName = $"{nodeBasePropertyName}{LinkTextPostfix}";

            return StringContentPropertyMatchesNodeProperty(
                TextFieldKey,
                contentItemField,
                nodeTextPropertyName,
                sourceNode);

            // string nodeUrlPropertyName = $"{nodeBasePropertyName}{_linkUrlPostfix}";
            // sourceNode.Properties.TryGetValue(nodeUrlPropertyName, out object? nodeUrlPropertyValue);
            //
            // // if (Convert.ToString(contentItemUrlFieldValue) != Convert.ToString(nodeUrlPropertyValue))
            // //     return false;
            //
            // JToken? contentItemUrlFieldValue = contentItemField?[_urlFieldKey];
            // if (contentItemUrlFieldValue == null || contentItemUrlFieldValue.Type == JTokenType.Null)
            // {
            //     return nodeUrlPropertyValue == null;
            // }
            //
            // if (nodeUrlPropertyValue == null)
            //     return false;
            //
            // return contentItemUrlFieldValue.As<string>() == (string)nodeUrlPropertyValue;
            //
            // JToken? contentItemTextFieldValue = contentItemField?[_textFieldKey];
            // string nodeTextPropertyName = $"{nodeBasePropertyName}{_linkTextPostfix}";
            // sourceNode.Properties.TryGetValue(nodeTextPropertyName, out object? nodeTextPropertyValue);
            //
            // if (Convert.ToString(contentItemTextFieldValue) != Convert.ToString(nodeTextPropertyValue))
            //     return false;
            //
            // return true;
        }
    }
}
