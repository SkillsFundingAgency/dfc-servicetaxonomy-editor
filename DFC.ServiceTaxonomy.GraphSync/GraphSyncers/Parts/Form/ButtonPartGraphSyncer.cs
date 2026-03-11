using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.Forms.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Form
{
    public class ButtonPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(ButtonPart);
        private static readonly Func<string, string> _formFieldsPropertyNameTransform = n => $"button_{n}";
        private const string TextPropertyName = "Text";
        private const string TypePropertyName = "Type";

        public override async Task AddSyncComponents(JsonObject content, IGraphMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            JsonValue? textValue = (JsonValue?)content[TextPropertyName];
            if (textValue != null && textValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(TextPropertyName), textValue.As<string>());

            JsonValue? typeValue = (JsonValue?)content[TypePropertyName];
            if (typeValue != null && typeValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(TypePropertyName), typeValue.As<string>());
        }

        public override async  Task<(bool validated, string failureReason)> ValidateSyncComponent(JsonObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TextPropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(TextPropertyName),
                context.NodeWithRelationships.SourceNode!);
            if (!matched)
                return (false, $"{TextPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TypePropertyName,
                content,
                await context.SyncNameProvider.PropertyName(TypePropertyName),
                context.NodeWithRelationships.SourceNode!);


            return matched ? (true, "") : (false, $"{TypePropertyName} did not validate: {failureReason}");
        }
    }
}
