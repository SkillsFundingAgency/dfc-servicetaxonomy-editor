using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.Forms.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Form
{
    public class FormPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(FormPart);
        private static readonly Func<string, string> _formFieldsPropertyNameTransform = n => $"form_{n}";
        private const string ActionPropertyName = "Action";
        private const string MethodPropertyName = "Method";
        private const string EncTypePropertyName = "EncType";
        private const string EnableAntiForgeryTokenPropertyName = "EnableAntiForgeryToken";
        private const string WorkflowTypeIdPropertyName = "WorkflowTypeId";

        public override async Task AddSyncComponents(JsonObject content, IGraphMergeContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            JsonValue? actionValue = (JsonValue?)content[ActionPropertyName];
            if (actionValue != null && actionValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(ActionPropertyName), actionValue.As<string>());

            JsonValue? methodValue = (JsonValue?)content[MethodPropertyName];
            if (methodValue != null && methodValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(MethodPropertyName), methodValue.As<string>());

            JsonValue? encTypeValue = (JsonValue?)content[EncTypePropertyName];
            if (encTypeValue != null && encTypeValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(EncTypePropertyName), encTypeValue.As<string>());

            JsonValue? workflowTypeIdValue = (JsonValue?)content[WorkflowTypeIdPropertyName];
            if (workflowTypeIdValue != null && workflowTypeIdValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(WorkflowTypeIdPropertyName), workflowTypeIdValue.As<string>());


            JsonValue? enableAntiForgeryTokenValue = (JsonValue?)content[EnableAntiForgeryTokenPropertyName];
            if (enableAntiForgeryTokenValue != null && enableAntiForgeryTokenValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(EnableAntiForgeryTokenPropertyName), enableAntiForgeryTokenValue.As<bool>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject content,
            IValidateAndRepairContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ActionPropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(ActionPropertyName),
                context.NodeWithRelationships.SourceNode!);
            if (!matched)
                return (false, $"{ActionPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                MethodPropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(MethodPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{MethodPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                EncTypePropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(EncTypePropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{EncTypePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                WorkflowTypeIdPropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(WorkflowTypeIdPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{WorkflowTypeIdPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                EnableAntiForgeryTokenPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(EnableAntiForgeryTokenPropertyName),
                context.NodeWithRelationships.SourceNode!);


            return matched ? (true, "") : (false, $"{EnableAntiForgeryTokenPropertyName} did not validate: {failureReason}");
        }
    }
}
