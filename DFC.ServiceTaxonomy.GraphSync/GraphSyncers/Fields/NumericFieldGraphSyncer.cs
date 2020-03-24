using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class NumericFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldName => "NumericField";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            JValue? value = (JValue?)contentItemField["Value"];
            if (value == null || !value.HasValues)
                return;

            var fieldSettings = contentPartFieldDefinition.GetSettings<NumericFieldSettings>();

            string propertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);
            if (fieldSettings.Scale == 0)
            {
                mergeNodeCommand.Properties.Add(propertyName, (int)value);
            }
            else
            {
                mergeNodeCommand.Properties.Add(propertyName, (decimal)value);
            }
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
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JToken? contentItemFieldValue = contentItemField?["Value"];

            if (contentItemFieldValue == null || !contentItemFieldValue.HasValues)
                return false;

            switch (nodePropertyValue)
            {
                case int i:
                    return contentItemFieldValue.Type == JTokenType.Integer && i == contentItemFieldValue.As<int>();
                case float f:
                    //todo: do we need to do this? if so set tolerance from scale settings
                    //todo: ude decimal instead?
                    //return contentItemFieldValue.Type == JTokenType.Float && Math.Abs(f - contentItemFieldValue.As<float>()) < 0.0001;
                    return contentItemFieldValue.Type == JTokenType.Float && f == contentItemFieldValue.As<float>();
                default:
                    //todo: log $"Found unexpected type {contentItemFieldValue.Type} in {contentPartFieldDefinition.Name} {FieldName}");
                    return false;
            }
        }
    }
}
