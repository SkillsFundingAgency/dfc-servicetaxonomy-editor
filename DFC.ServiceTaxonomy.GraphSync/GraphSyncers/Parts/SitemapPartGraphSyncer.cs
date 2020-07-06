using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
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

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_sitemapPropertyNameTransform);

            //todo: helper for these?
            JValue? value = (JValue?)content[OverrideSitemapConfigPropertyName];
            if (value != null && value.Type != JTokenType.Null) //first bool?
                context.MergeNodeCommand.Properties.Add(await context.GraphSyncHelper.PropertyName(OverrideSitemapConfigPropertyName), value.As<bool>());

            value = (JValue?)content[ChangeFrequencyPropertyName];
            if (value != null && value.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.GraphSyncHelper.PropertyName(ChangeFrequencyPropertyName), ((ChangeFrequency)value.As<int>()).ToString().ToLowerInvariant());

            value = (JValue?)content[PriorityPropertyName];
            if (value != null && value.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.GraphSyncHelper.PropertyName(PriorityPropertyName), value.As<int>());

            value = (JValue?)content[ExcludePropertyName];
            if (value != null && value.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.GraphSyncHelper.PropertyName(ExcludePropertyName), value.As<bool>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_sitemapPropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                OverrideSitemapConfigPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(OverrideSitemapConfigPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{OverrideSitemapConfigPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.EnumContentPropertyMatchesNodeProperty<ChangeFrequency>(
                ChangeFrequencyPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(ChangeFrequencyPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{ChangeFrequencyPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.LongContentPropertyMatchesNodeProperty(
                PriorityPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(PriorityPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{PriorityPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ExcludePropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(ExcludePropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            return matched ? (true, "") : (false, $"{ExcludePropertyName} did not validate: {failureReason}");
        }
    }

#pragma warning restore S1481
}
