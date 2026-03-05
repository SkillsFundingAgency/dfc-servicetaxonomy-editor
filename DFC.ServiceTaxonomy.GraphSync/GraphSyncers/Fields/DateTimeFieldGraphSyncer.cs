using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class DateTimeFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "DateTimeField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JsonObject contentItemField, IGraphMergeContext context)
        {
            JsonValue? value = (JsonValue?)contentItemField?[ContentKey];
            if (value == null || value.GetValueKind() == JsonValueKind.Null)
                return;

            string propertyName = await context.SyncNameProvider!.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.Properties.Add(propertyName, (DateTime)value);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JsonObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
               ContentKey,
               contentItemField,
               nodePropertyName,
               context.NodeWithRelationships.SourceNode!);
        }
    }
}
