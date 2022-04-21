using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields
{
    public class HtmlFieldDataSyncer : IContentFieldDataSyncer
    {
        public string FieldTypeName => "HtmlField";

        private const string ContentKey = "Html";

        public async Task AddSyncComponents(JObject contentItemField, IDataMergeContext context)
        {
            JValue? value = (JValue?)contentItemField[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            context.MergeNodeCommand.Properties.Add(
                await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name),
                value.As<string>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }
    }
}
