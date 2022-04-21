using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields
{
    public class DateTimeFieldDataSyncer : IContentFieldDataSyncer
    {
        public string FieldTypeName => "DateTimeField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JObject contentItemField, IDataMergeContext context)
        {
            JValue? value = (JValue?)contentItemField?[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            string propertyName = await context.SyncNameProvider!.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.Properties.Add(propertyName, (DateTime)value);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.DataSyncValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
               ContentKey,
               contentItemField,
               nodePropertyName,
               context.NodeWithRelationships.SourceNode!);
        }
    }
}
