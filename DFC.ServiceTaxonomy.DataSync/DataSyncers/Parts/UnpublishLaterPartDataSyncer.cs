using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class UnpublishLaterPartDataSyncer : ContentPartDataSyncer
    {
        public override string PartName => nameof(UnpublishLaterPart);

        private const string _contentTitlePropertyName = "ScheduledUnpublishUtc";

        //todo: configurable??
        public const string NodeTitlePropertyName = "unpublishlater_ScheduledUnpublishUtc";

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<DateTime>(NodeTitlePropertyName, content, _contentTitlePropertyName);

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.DataSyncValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithRelationships.SourceNode!));
        }
    }
#pragma warning restore S1481
}
