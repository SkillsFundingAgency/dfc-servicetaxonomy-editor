using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class DateTimeFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "DateTimeField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            JValue? value = (JValue?)contentItemField?[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            string propertyName = await context.GraphSyncHelper!.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.Properties.Add(propertyName, (DateTime)value);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            ValidateAndRepairContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
               ContentKey,
               contentItemField,
               nodePropertyName,
               context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
}
