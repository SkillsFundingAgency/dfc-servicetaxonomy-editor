using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.Forms.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Form
{
    public class InputPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(InputPart);
        private static readonly Func<string, string> _formFieldsPropertyNameTransform = n => $"input_{n}";
        public const string TypePropertyName = "Type";
        public const string DefaultValuePropertyName = "DefaultValue";
        public const string PlaceholderPropertyName = "Placeholder";
        public override async Task AddSyncComponents(JsonObject content, IGraphMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            JsonValue? typeValue = (JsonValue?)content[TypePropertyName];
            if (typeValue != null && typeValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(TypePropertyName), typeValue.As<string>());

            JsonValue? defaultValue = (JsonValue?)content[DefaultValuePropertyName];
            if (defaultValue != null && defaultValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(DefaultValuePropertyName), defaultValue.As<string>());

            JsonValue? placeholderValue = (JsonValue?)content[DefaultValuePropertyName];
            if (placeholderValue != null && placeholderValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(DefaultValuePropertyName), placeholderValue.As<string>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JsonObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TypePropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(TypePropertyName),
                context.NodeWithRelationships.SourceNode!);
            if (!matched)
                return (false, $"{TypePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                DefaultValuePropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(DefaultValuePropertyName),
                context.NodeWithRelationships.SourceNode!);
            if (!matched)
                return (false, $"{DefaultValuePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                PlaceholderPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(PlaceholderPropertyName),
                context.NodeWithRelationships.SourceNode!);


            return matched ? (true, "") : (false, $"{PlaceholderPropertyName} did not validate: {failureReason}");
        }
    }
}
