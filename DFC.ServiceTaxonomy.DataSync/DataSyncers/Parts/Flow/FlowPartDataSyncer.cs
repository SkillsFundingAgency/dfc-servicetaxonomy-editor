using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Flow
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class FlowPartDataSyncer : EmbeddingContentPartDataSyncer
    {
        private readonly IContentFieldsDataSyncer _contentFieldsDataSyncer;

        public override string PartName => nameof(FlowPart);
        protected override string ContainerName => "Widgets";

        private static readonly Func<string, string> _flowFieldsPropertyNameTransform = n => $"flow_{n}";

        public FlowPartDataSyncer(
            IFlowPartEmbeddedContentItemsDataSyncer flowPartEmbeddedContentItemsDataSyncer,
            IContentFieldsDataSyncer contentFieldsDataSyncer)
        : base(flowPartEmbeddedContentItemsDataSyncer)
        {
            _contentFieldsDataSyncer = contentFieldsDataSyncer;
        }

        public override Task AllowSync(JObject content, IDataMergeContext context, IAllowSync allowSync)
        {
            return Task.WhenAll(
                base.AllowSync(content, context, allowSync),
                _contentFieldsDataSyncer.AllowSync(content, context, allowSync));
        }

        public override async Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            //todo: make concurrent?
            await base.AddSyncComponents(content, context);

            // FlowPart allows part definition level fields, but values are on each FlowPart instance
            // prefix flow field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_flowFieldsPropertyNameTransform);

            await _contentFieldsDataSyncer.AddSyncComponents(content, context);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            (bool validated, string failureReason) =
                await base.ValidateSyncComponent(content, context);

            if (!validated)
                return (validated, failureReason);

            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_flowFieldsPropertyNameTransform);

            return await _contentFieldsDataSyncer.ValidateSyncComponent(
                content, context);
        }
    }

#pragma warning restore S1481
}
