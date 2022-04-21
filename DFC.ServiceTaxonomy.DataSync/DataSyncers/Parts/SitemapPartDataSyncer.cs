using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;
using OrchardCore.Sitemaps.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class SitemapPartDataSyncer : ContentPartDataSyncer
    {
        public override string PartName => nameof(SitemapPart);

        private static readonly Func<string, string> _sitemapPropertyNameTransform = n => $"sitemap_{n}";

        private const string
            OverrideSitemapConfigPropertyName = "OverrideSitemapConfig",
            ChangeFrequencyPropertyName = "ChangeFrequency",
            PriorityPropertyName = "Priority",
            ExcludePropertyName = "Exclude";

        public override async Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_sitemapPropertyNameTransform);

            //todo: helper for these?
            JValue? value = (JValue?)content[OverrideSitemapConfigPropertyName];
            if (value != null && value.Type != JTokenType.Null) //first bool?
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(OverrideSitemapConfigPropertyName), value.As<bool>());

            //todo: we want the change frequency value lowercase (as the sitemap xml format wants it lowercase),
            // but we probably want enums to serialise using normal casing, so can Enum.Parse etc.
            // add flag to any enum helpers, such as EnumContentPropertyMatchesNodeProperty
            value = (JValue?)content[ChangeFrequencyPropertyName];
            if (value != null && value.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(ChangeFrequencyPropertyName), ((ChangeFrequency)value.As<int>()).ToString().ToLowerInvariant());

            value = (JValue?)content[PriorityPropertyName];
            if (value != null && value.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(PriorityPropertyName), value.As<int>());

            value = (JValue?)content[ExcludePropertyName];
            if (value != null && value.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(ExcludePropertyName), value.As<bool>());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_sitemapPropertyNameTransform);

            (bool matched, string failureReason) = context.DataSyncValidationHelper.BoolContentPropertyMatchesNodeProperty(
                OverrideSitemapConfigPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(OverrideSitemapConfigPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{OverrideSitemapConfigPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.DataSyncValidationHelper.EnumContentPropertyMatchesNodeProperty<ChangeFrequency>(
                ChangeFrequencyPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(ChangeFrequencyPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{ChangeFrequencyPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.DataSyncValidationHelper.LongContentPropertyMatchesNodeProperty(
                PriorityPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(PriorityPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{PriorityPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.DataSyncValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ExcludePropertyName,
                content,
                await context.SyncNameProvider.PropertyName(ExcludePropertyName),
                context.NodeWithRelationships.SourceNode!);

            return matched ? (true, "") : (false, $"{ExcludePropertyName} did not validate: {failureReason}");
        }
    }

#pragma warning restore S1481
}
