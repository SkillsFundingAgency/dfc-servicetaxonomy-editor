using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class NumericFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "NumericField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            JValue? value = (JValue?)contentItemField?[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            var fieldSettings = contentPartFieldDefinition.GetSettings<NumericFieldSettings>();

            string propertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);
            if (fieldSettings.Scale == 0)
            {
                mergeNodeCommand.Properties.Add(propertyName, (int)value);
            }
            else
            {
                //todo: check this
                mergeNodeCommand.Properties.Add(propertyName, (decimal)value);
            }
        }

        public async Task<bool> VerifySyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JToken? contentItemFieldValue = contentItemField[ContentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                return nodePropertyValue == null;
            }

            if (nodePropertyValue == null)
                return false;

            var fieldSettings = contentPartFieldDefinition.GetSettings<NumericFieldSettings>();

            if (fieldSettings.Scale == 0)
            {
                //todo: think we only currently need to support JTokenType.Integer coz the import util doesn't generate values the same way as OC
                // if (contentItemFieldValue.Type == JTokenType.Float
                //     || contentItemFieldValue.Type == JTokenType.Integer)

                return nodePropertyValue is int nodePropertyValueInt
                       && nodePropertyValueInt == (int)contentItemFieldValue;

                // if (nodePropertyValue is int nodePropertyValueInt)
                //     return nodePropertyValueInt == (int)contentItemFieldValue;
                //
                // return false;
                //bool nodePropertyValueIsWholeNumber = Math.Abs(nodePropertyValue % 1) <= Double.Epsilon * 100;
            }

            // think we only currently need to support JTokenType.Integer coz the import util doesn't generate values the same way as OC
            //todo: fix importer
            // return (contentItemFieldValue.Type == JTokenType.Float || contentItemFieldValue.Type == JTokenType.Integer)
            //        && (decimal)nodePropertyValue == (decimal)contentItemFieldValue;

            float allowableDifference = 1f / (float)BigInteger.Pow(10, fieldSettings.Scale + 2);

            return nodePropertyValue is float nodePropertyValueFloat
                && Math.Abs(nodePropertyValueFloat - (float)contentItemFieldValue) <= allowableDifference;

            // switch (nodePropertyValue)
            // {
            //     case int i:
            //         return contentItemFieldValue.Type == JTokenType.Integer && i == contentItemFieldValue.As<int>();
            //     case float f:
            //         //todo: do we need to do this? if so set tolerance from scale settings
            //         //todo: ude decimal instead?
            //         //return contentItemFieldValue.Type == JTokenType.Float && Math.Abs(f - contentItemFieldValue.As<float>()) < 0.0001;
            //         return contentItemFieldValue.Type == JTokenType.Float && f == contentItemFieldValue.As<decimal>();
            //     default:
            //         //todo: log $"Found unexpected type {contentItemFieldValue.Type} in {contentPartFieldDefinition.Name} {FieldName}");
            //         return false;
            // }
        }
    }
}
