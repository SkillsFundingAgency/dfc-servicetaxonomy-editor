using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.Html.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class HtmlBodyPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(HtmlBodyPart);

        private static readonly Func<string, string> _htmlBodyFieldsPropertyNameTransform = n => $"htmlbody_{n}";
        private const string HtmlPropertyName = "Html";

        public override async Task AddSyncComponents(JsonObject content, IGraphMergeContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_htmlBodyFieldsPropertyNameTransform);

            JsonValue? htmlValue = (JsonValue?)content[HtmlPropertyName];
            if (htmlValue != null && htmlValue.GetValueKind() != JsonValueKind.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(HtmlPropertyName), htmlValue.As<string>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject content,
            IValidateAndRepairContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_htmlBodyFieldsPropertyNameTransform);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                HtmlPropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(HtmlPropertyName),
                context.NodeWithRelationships.SourceNode!);
        }
    }
#pragma warning restore S1481
}
