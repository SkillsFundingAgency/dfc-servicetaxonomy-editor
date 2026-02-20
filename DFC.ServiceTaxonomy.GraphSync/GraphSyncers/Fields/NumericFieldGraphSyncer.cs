using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class NumericFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "NumericField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JsonObject contentItemField, IGraphMergeContext context)
        {
            JsonValue? value = (JsonValue?)contentItemField?[ContentKey];
            if (value == null || value.GetValueKind() == JsonValueKind.Null)
                return;

            var fieldSettings = context.ContentPartFieldDefinition!.GetSettings<NumericFieldSettings>();

            string propertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);
            if (fieldSettings.Scale == 0)
            {
                context.MergeNodeCommand.Properties.Add(propertyName, (int)value);
            }
            else
            {
                context.MergeNodeCommand.Properties.Add(propertyName, (decimal)value);
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject contentItemField,
            IValidateAndRepairContext context)
        {
            //todo: use/move to GraphValidationHelper?

            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);
            context.NodeWithRelationships.SourceNode!.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            var contentItemFieldValue = contentItemField[ContentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.GetValueKind() == JsonValueKind.Null)
            {
                bool bothNull = nodePropertyValue == null;
                return (bothNull, $"content property value was null, but node property value was not null (numeric - {nodePropertyName} - {nodePropertyValue})");
            }

            if (nodePropertyValue == null)
                return (false, "node property value was null, but content property value was not null");

            var fieldSettings = context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>();

            if (fieldSettings.Scale == 0)
            {
                bool longsSame;

                if (!long.TryParse(nodePropertyValue.ToString(), out long nodePropertyValueLong))
                {
                    longsSame = false;
                }
                else
                {
                    longsSame = nodePropertyValueLong == (long)contentItemFieldValue;
                }
                return (longsSame, longsSame?"":$"long content property value was '{contentItemFieldValue}', but node property value was '{nodePropertyValue}'");
            }

            // calculate allowable tolerance from scale setting
            double allowableDifference = 1d / Math.Pow(10d, fieldSettings.Scale + 2);

            bool doublesSame;
            if (!double.TryParse(nodePropertyValue.ToString(), out double nodePropertyValueDouble))
            {
                doublesSame = false;
            }
            else
            {
                doublesSame = Math.Abs(nodePropertyValueDouble - (double)contentItemFieldValue) <= allowableDifference;
            }

            return (doublesSame, doublesSame?"":$"double content property value was '{contentItemFieldValue}', but node property value was '{nodePropertyValue}' and allowable difference was {allowableDifference}");
        }
    }
}
