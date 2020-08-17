using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class NumericFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "NumericField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            JValue? value = (JValue?)contentItemField?[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            var fieldSettings = context.ContentPartFieldDefinition!.GetSettings<NumericFieldSettings>();

            string propertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);
            if (fieldSettings.Scale == 0)
            {
                context.MergeNodeCommand.Properties.Add(propertyName, (int)value);
            }
            else
            {
                context.MergeNodeCommand.Properties.Add(propertyName, (decimal)value);
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);
            context.NodeWithOutgoingRelationships.SourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JToken? contentItemFieldValue = contentItemField[ContentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                bool bothNull = nodePropertyValue == null;
                return (bothNull, "content property value was null, but node property value was not null");
            }

            if (nodePropertyValue == null)
                return (false, "node property value was null, but content property value was not null");

            var fieldSettings = context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>();

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
