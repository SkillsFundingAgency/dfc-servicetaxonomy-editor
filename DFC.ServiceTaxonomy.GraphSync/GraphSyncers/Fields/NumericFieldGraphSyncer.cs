using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
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

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
        {
            string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);
            nodeWithOutgoingRelationships.SourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JToken? contentItemFieldValue = contentItemField[ContentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                bool bothNull = nodePropertyValue == null;
                return (bothNull, "content property value was null, but node property value was not null");
            }

            if (nodePropertyValue == null)
                return (false, "node property value was null, but content property value was not null");

            var fieldSettings = contentPartFieldDefinition.GetSettings<NumericFieldSettings>();

            if (fieldSettings.Scale == 0)
            {
                bool longsSame = nodePropertyValue is long nodePropertyValueInt
                       && nodePropertyValueInt == (long)contentItemFieldValue;
                return (longsSame, longsSame?"":$"long content property value was '{contentItemFieldValue}', but node property value was '{nodePropertyValue}'");
            }

            // calculate allowable tolerance from scale setting
            double allowableDifference = 1d / Math.Pow(10d, fieldSettings.Scale + 2);

            bool doublesSame = nodePropertyValue is double nodePropertyValueFloat
                && Math.Abs(nodePropertyValueFloat - (double)contentItemFieldValue) <= allowableDifference;

            return (doublesSame, doublesSame?"":$"double content property value was '{contentItemFieldValue}', but node property value was '{nodePropertyValue}' and allowable difference was {allowableDifference}");
        }
    }
}
