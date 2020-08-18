using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.Html.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class HtmlBodyPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(HtmlBodyPart);

        private static readonly Func<string, string> _htmlBodyFieldsPropertyNameTransform = n => $"htmlbody_{n}";
        private const string HtmlPropertyName = "Html";

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_htmlBodyFieldsPropertyNameTransform);

            JValue? htmlValue = (JValue?)content[HtmlPropertyName];
            if (htmlValue != null && htmlValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.GraphSyncHelper.PropertyName(HtmlPropertyName), htmlValue.As<string>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_htmlBodyFieldsPropertyNameTransform);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                HtmlPropertyName,
                content,
                await context.GraphSyncHelper!.PropertyName(HtmlPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
#pragma warning restore S1481
}
