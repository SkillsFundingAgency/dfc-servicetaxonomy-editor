using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.PublishLater.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class PublishLaterPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(PublishLaterPart);

        private static readonly Func<string, string> _publishLaterFieldsPropertyNameTransform = n => $"publishlater_{n}";
        private const string ScheduledPublishUtcPropertyName = "ScheduledPublishUtc";

        public async Task AddSyncComponents(JObject content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = graphSyncHelper.PushPropertyNameTransform(_publishLaterFieldsPropertyNameTransform);

            JValue? scheduledPublishValue = (JValue?)content[ScheduledPublishUtcPropertyName];
            if (scheduledPublishValue != null && scheduledPublishValue.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(await graphSyncHelper!.PropertyName(ScheduledPublishUtcPropertyName), scheduledPublishValue.As<DateTime>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = graphSyncHelper.PushPropertyNameTransform(_publishLaterFieldsPropertyNameTransform);

            return graphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                ScheduledPublishUtcPropertyName,
                content,
                await graphSyncHelper!.PropertyName(ScheduledPublishUtcPropertyName),
                nodeWithOutgoingRelationships.SourceNode);
        }
    }
#pragma warning restore S1481
}
