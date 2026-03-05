using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class LinkFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "LinkField";

        private const string UrlFieldKey = "Url", TextFieldKey = "Text";
        private const string LinkUrlPostfix = "_url", LinkTextPostfix = "_text";

        public async Task AddSyncComponents(JsonObject contentItemField, IGraphMergeContext context)
        {
            string basePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            JsonValue? value = (JsonValue?)contentItemField[UrlFieldKey];
            if (value != null && value.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add($"{basePropertyName}{LinkUrlPostfix}", value.As<string>());

            value = (JsonValue?)contentItemField[TextFieldKey];
            if (value != null && value.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add($"{basePropertyName}{LinkTextPostfix}", value.As<string>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JsonObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodeBasePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            string nodeUrlPropertyName = $"{nodeBasePropertyName}{LinkUrlPostfix}";

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                UrlFieldKey,
                contentItemField,
                nodeUrlPropertyName,
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"url did not validate: {failureReason}");

            string nodeTextPropertyName = $"{nodeBasePropertyName}{LinkTextPostfix}";

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TextFieldKey,
                contentItemField,
                nodeTextPropertyName,
                context.NodeWithRelationships.SourceNode!);

            return (matched, matched ? "" : $"text did not validate: {failureReason}");
        }
    }
}
