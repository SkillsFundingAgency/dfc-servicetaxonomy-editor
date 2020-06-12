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

    public class SitemapPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(SitemapPart);

        private static readonly Func<string, string> _sitemapPropertyNameTransform = n => $"sitemap_{n}";

        private const string
            OverrideSitemapConfigPropertyName = "OverrideSitemapConfig",
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
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(OverrideSitemapConfigPropertyName), value.As<bool>());

            value = content.ChangeFrequency;
            if (value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(ChangeFrequencyPropertyName), ((ChangeFrequency)value.As<int>()).ToString());

            value = content.Priority;
            if (value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(await graphSyncHelper.PropertyName(PriorityPropertyName), value.As<int>()/10m);

            value = content.Exclude;
            if (value.Type != JTokenType.Null)
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

            (bool matched, string failureReason) = graphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                OverrideSitemapConfigPropertyName,
                content,
                await graphSyncHelper.PropertyName(OverrideSitemapConfigPropertyName),
                nodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{OverrideSitemapConfigPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = graphValidationHelper.EnumContentPropertyMatchesNodeProperty<ChangeFrequency>(
                ChangeFrequencyPropertyName,
                content,
                await graphSyncHelper.PropertyName(ChangeFrequencyPropertyName),
                nodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{ChangeFrequencyPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = graphValidationHelper.ContentPropertyMatchesNodeProperty(
                PriorityPropertyName,
                content,
                await graphSyncHelper.PropertyName(PriorityPropertyName),
                nodeWithOutgoingRelationships.SourceNode,
                (contentValue, nodeValue) => Equals(((int)contentValue)/10m, (decimal)nodeValue));

            if (!matched)
                return (false, $"{PriorityPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = graphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ExcludePropertyName,
                content,
                await graphSyncHelper.PropertyName(ExcludePropertyName),
                nodeWithOutgoingRelationships.SourceNode);

            return matched ? (true, "") : (false, $"{ExcludePropertyName} did not validate: {failureReason}");
        }
    }

#pragma warning restore S1481
}
