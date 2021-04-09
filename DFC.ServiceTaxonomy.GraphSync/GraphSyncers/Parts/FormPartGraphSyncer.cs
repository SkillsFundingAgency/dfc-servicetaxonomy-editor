using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Models;


namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235
    class FormPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(FormPart);
        private static readonly Func<string, string> _formFieldsPropertyNameTransform = n => $"form_{n}";
        private const string ActionPropertyName = "Action";
        private const string MethodPropertyName = "Method";
        private const string EncTypePropertyName = "EncType";
        private const string EnableAntiForgeryTokenPropertyName = "EnableAntiForgeryToken";
        private const string WorkflowTypeIdPropertyName = "WorkflowTypeId";



        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            JValue? actionValue = (JValue?)content[ActionPropertyName];
            if (actionValue != null && actionValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(ActionPropertyName), actionValue.As<string>());

            JValue? methodValue = (JValue?)content[MethodPropertyName];
            if (methodValue != null && methodValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(MethodPropertyName), methodValue.As<string>());

            JValue? encTypeValue = (JValue?)content[EncTypePropertyName];
            if (encTypeValue != null && encTypeValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(EncTypePropertyName), encTypeValue.As<string>());

            JValue? workflowTypeIdValue = (JValue?)content[WorkflowTypeIdPropertyName];
            if (workflowTypeIdValue != null && workflowTypeIdValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(WorkflowTypeIdPropertyName), workflowTypeIdValue.As<string>());


            JValue? enableAntiForgeryTokenValue = (JValue?)content[EnableAntiForgeryTokenPropertyName];
            if (enableAntiForgeryTokenValue != null && enableAntiForgeryTokenValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(EnableAntiForgeryTokenPropertyName), enableAntiForgeryTokenValue.As<bool>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
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
