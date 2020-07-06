using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.PublishLater.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class PublishLaterPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(PublishLaterPart);

        private static readonly Func<string, string> _publishLaterFieldsPropertyNameTransform = n => $"publishlater_{n}";
        private const string ScheduledPublishUtcPropertyName = "ScheduledPublishUtc";

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_publishLaterFieldsPropertyNameTransform);

            JValue? scheduledPublishValue = (JValue?)content[ScheduledPublishUtcPropertyName];
            if (scheduledPublishValue != null && scheduledPublishValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.GraphSyncHelper.PropertyName(ScheduledPublishUtcPropertyName), scheduledPublishValue.As<DateTime>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ValidateAndRepairContext context)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_publishLaterFieldsPropertyNameTransform);

            return context.GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                ScheduledPublishUtcPropertyName,
                content,
                await context.GraphSyncHelper!.PropertyName(ScheduledPublishUtcPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
#pragma warning restore S1481
}
