using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class UnpublishLaterPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(UnpublishLaterPart);

        private const string _contentTitlePropertyName = "ScheduledUnpublishUtc";

        //todo: configurable??
        public const string NodeTitlePropertyName = "unpublishlater_ScheduledUnpublishUtc";

        public Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<DateTime>(NodeTitlePropertyName, content, _contentTitlePropertyName);

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode));
        }
    }
#pragma warning restore S1481
}
