using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class LinkFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldName => "LinkField";

        private const string _urlFieldKey = "Url", _textFieldKey = "Text";
        private const string _linkUrlPostfix = "_url", _linkTextPostfix = "_text";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            string basePropertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);

            //check what we get if no value entered. should we fail sync if null?
            string? url = contentItemField[_urlFieldKey]?.ToString();
            //todo:             if (value == null || !value.HasValues)

            if (url != null)
                mergeNodeCommand.Properties.Add($"{basePropertyName}{_linkUrlPostfix}", url);

            string? text = contentItemField[_textFieldKey]?.ToString();
            if (text != null)
                mergeNodeCommand.Properties.Add($"{basePropertyName}{_linkTextPostfix}", text);
        }

        public async Task<bool> VerifySyncComponent(
            JObject contentItemField,
            //ContentTypePartDefinition contentTypePartDefinition,
            ContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            string nodeBasePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);

            JToken? contentItemUrlFieldValue = contentItemField?[_urlFieldKey];
            string nodeUrlPropertyName = $"{nodeBasePropertyName}{_linkUrlPostfix}";
            sourceNode.Properties.TryGetValue(nodeUrlPropertyName, out object? nodeUrlPropertyValue);

            if (Convert.ToString(contentItemUrlFieldValue) != Convert.ToString(nodeUrlPropertyValue))
                return false;

            JToken? contentItemTextFieldValue = contentItemField?[_textFieldKey];
            string nodeTextPropertyName = $"{nodeBasePropertyName}{_linkTextPostfix}";
            sourceNode.Properties.TryGetValue(nodeTextPropertyName, out object? nodeTextPropertyValue);

            if (Convert.ToString(contentItemTextFieldValue) != Convert.ToString(nodeTextPropertyValue))
                return false;

            return true;
        }
    }
}
