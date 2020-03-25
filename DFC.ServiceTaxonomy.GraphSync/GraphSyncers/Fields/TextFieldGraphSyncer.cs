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
    //todo: better name to reflect only for PropertyFieldGraphSyncer??
    //todo: inheritance/composition/extension??
    public class FieldGraphSyncer
    {
        //todo: better name
        //todo: better distinguish between fieldTYPEname and fieldname
        //todo: log validation failed reasons
        protected bool StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            //string nodePropertyName = await graphSyncHelper.PropertyName(fieldName); //contentPartFieldDefinition.Name);
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JValue? contentItemFieldValue = (JValue?)contentItemField?[contentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                return nodePropertyValue == null;
            }

            if (nodePropertyValue == null)
            {
                return false;
            }

            return contentItemFieldValue.As<string>() == (string)nodePropertyValue;
        }
    }

    public class TextFieldGraphSyncer : FieldGraphSyncer, IContentFieldGraphSyncer
    {
        public string FieldTypeName => "TextField";

        private const string ContentKey = "Text";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            //todo: helper for this?
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

//             string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);
//             sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);
//
//             JToken? contentItemFieldValue = contentItemField?["Text"];
// //todo: need to distinguish between null and empty string : they're not equivalent
//             return Convert.ToString(contentItemFieldValue) == Convert.ToString(nodePropertyValue);

            // string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);
            // sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);
            //
            // JValue? contentItemFieldValue = (JValue?)contentItemField[ContentKey];
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
