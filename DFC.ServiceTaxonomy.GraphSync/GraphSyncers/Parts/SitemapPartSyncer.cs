using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class SitemapPartSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(SitemapPart);

        private static readonly Func<string, string> _sitemapPropertyNameTransform = n => $"sitemap_{n}";

        private const string
            OverrideConfigPropertyName = "OverrideConfig",
            ChangeFrequencyPropertyName = "ChangeFrequency",
            PriorityPropertyName = "Priority",
            ExcludePropertyName = "Exclude";

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            using var _ = graphSyncHelper.PushPropertyNameTransform(_sitemapPropertyNameTransform);

            //todo: helper for these?
            JValue value = content.OverrideSitemapConfig;
            if (value.Type != JTokenType.Null) //first bool?
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(OverrideConfigPropertyName), value.As<bool>());

            value = content.ChangeFrequency;
            if (value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(ChangeFrequencyPropertyName), ((ChangeFrequency)value.As<int>()).ToString());

            value = content.Priority;
            if (value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(PriorityPropertyName), (long)value.As<int>());

            value = content.Exclude;
            if (value.Type != JTokenType.Null) //first bool?
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(ExcludePropertyName), value.As<bool>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            using var _ = graphSyncHelper.PushPropertyNameTransform(_sitemapPropertyNameTransform);

            (bool matched, string failureReason) = graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                OverrideConfigPropertyName,
                content,
                await graphSyncHelper.PropertyName(OverrideConfigPropertyName),
                nodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{OverrideConfigPropertyName} did not validate: {failureReason}");

            //todo: others

            return (true, "");
        }
    }

#pragma warning restore S1481
}
