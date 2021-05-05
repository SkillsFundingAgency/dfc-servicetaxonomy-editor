using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
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
        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            JValue? typeValue = (JValue?)content[TypePropertyName];
            if (typeValue != null && typeValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(TypePropertyName), typeValue.As<string>());

            JValue? defaultValue = (JValue?)content[DefaultValuePropertyName];
            if (defaultValue != null && defaultValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(DefaultValuePropertyName), defaultValue.As<string>());

            JValue? placeholderValue = (JValue?)content[DefaultValuePropertyName];
            if (placeholderValue != null && placeholderValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(DefaultValuePropertyName), placeholderValue.As<string>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
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
