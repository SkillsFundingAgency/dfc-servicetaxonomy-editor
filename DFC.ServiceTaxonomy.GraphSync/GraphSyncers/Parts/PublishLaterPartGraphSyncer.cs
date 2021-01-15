using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;
using OrchardCore.PublishLater.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class PublishLaterPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(PublishLaterPart);

        private static readonly Func<string, string> _publishLaterFieldsPropertyNameTransform = n => $"publishlater_{n}";
        private const string ScheduledPublishUtcPropertyName = "ScheduledPublishUtc";

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_publishLaterFieldsPropertyNameTransform);

            context.MergeNodeCommand.AddProperty<DateTime>(
                await context.SyncNameProvider.PropertyName(ScheduledPublishUtcPropertyName),
                content, ScheduledPublishUtcPropertyName);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_publishLaterFieldsPropertyNameTransform);

            return context.GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                ScheduledPublishUtcPropertyName,
                content,
                await context.SyncNameProvider!.PropertyName(ScheduledPublishUtcPropertyName),
                context.NodeWithRelationships.SourceNode!);
        }
    }
#pragma warning restore S1481
}
