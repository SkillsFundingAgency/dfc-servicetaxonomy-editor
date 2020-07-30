using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using Newtonsoft.Json.Linq;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Flow
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class FlowPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IFlowPartEmbeddedContentItemsGraphSyncer _flowPartEmbeddedContentItemsGraphSyncer;
        private readonly IContentFieldsGraphSyncer _contentFieldsGraphSyncer;

        public string PartName => nameof(FlowPart);

        private const string ContainerName = "Widgets";
        private static readonly Func<string, string> _flowFieldsPropertyNameTransform = n => $"flow_{n}";

        public FlowPartGraphSyncer(
            IFlowPartEmbeddedContentItemsGraphSyncer flowPartEmbeddedContentItemsGraphSyncer,
            IContentFieldsGraphSyncer contentFieldsGraphSyncer)
        {
            _flowPartEmbeddedContentItemsGraphSyncer = flowPartEmbeddedContentItemsGraphSyncer;
            _contentFieldsGraphSyncer = contentFieldsGraphSyncer;
        }

        public async Task AllowSync(JArray? contentItems, IGraphMergeContext context, IAllowSyncResult allowSyncResult)
        {
            await Task.WhenAll(
                _flowPartEmbeddedContentItemsGraphSyncer.AllowSync(contentItems, context, allowSyncResult),
                _contentFieldsGraphSyncer.AllowSync(contentItems, context, allowSyncResult));
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            //todo: make concurrent
            await _flowPartEmbeddedContentItemsGraphSyncer.AddSyncComponents((JArray?)content[ContainerName], context);

            // FlowPart allows part definition level fields, but values are on each FlowPart instance
            // prefix flow field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_flowFieldsPropertyNameTransform);

            await _contentFieldsGraphSyncer.AddSyncComponents(content, context);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            (bool validated, string failureReason) =
                await _flowPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName], context);

            if (!validated)
                return (validated, failureReason);

            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_flowFieldsPropertyNameTransform);

            return await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content, context);
        }
    }

#pragma warning restore S1481
}
