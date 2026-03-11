using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class HtmlFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "HtmlField";

        private const string ContentKey = "Html";

        public async Task AddSyncComponents(JsonObject contentItemField, IGraphMergeContext context)
        {
            JsonValue? value = (JsonValue?)contentItemField[ContentKey];
            if (value == null || value.GetValueKind() == JsonValueKind.Null)
                return;

            context.MergeNodeCommand.Properties.Add(
                await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name),
                value.As<string>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JsonObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }
    }
}
