using System;
using System.Collections.Generic;
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
                mergeNodeCommand.Properties.Add(propertyName, (decimal)value);
            }
        }

        public async Task<bool> VerifySyncComponent(
            JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper)
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
                return nodePropertyValue is long nodePropertyValueInt
                       && nodePropertyValueInt == (long)contentItemFieldValue;
            }

            // calculate allowable tolerance from scale setting
            double allowableDifference = 1d / Math.Pow(10d, fieldSettings.Scale + 2);

            return nodePropertyValue is double nodePropertyValueFloat
                && Math.Abs(nodePropertyValueFloat - (double)contentItemFieldValue) <= allowableDifference;
        }
    }
}
