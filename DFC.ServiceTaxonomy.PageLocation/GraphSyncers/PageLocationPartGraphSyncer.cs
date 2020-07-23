using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.PageLocation.GraphSyncers
{
#pragma warning disable S1481 // Unused local variables should be removed
    public class PageLocationPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(PageLocationPart);

        private static readonly Func<string, string> _pageLocationPropertyNameTransform = n => $"pagelocation_{n}";

        private const string
            UrlNamePropertyName = "UrlName",
            DefaultPageForLocationPropertyName = "DefaultPageForLocation",
            FullUrlPropertyName = "FullUrl",
            RedirectLocationsPropertyName = "RedirectLocations";

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_pageLocationPropertyNameTransform);

            context.MergeNodeCommand.AddProperty<string>(await context.GraphSyncHelper.PropertyName(UrlNamePropertyName), content, UrlNamePropertyName);
            context.MergeNodeCommand.AddProperty<bool>(await context.GraphSyncHelper.PropertyName(DefaultPageForLocationPropertyName), content, DefaultPageForLocationPropertyName);
            context.MergeNodeCommand.AddProperty<string>(await context.GraphSyncHelper.PropertyName(FullUrlPropertyName), content, FullUrlPropertyName);

            var val = content["RedirectLocations"]?.ToString().Split("\r\n");

            if (val != null)
            {
                var array = JArray.FromObject(val);
                context.MergeNodeCommand.AddArrayProperty<string>(await context.GraphSyncHelper.PropertyName(RedirectLocationsPropertyName), array);
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_pageLocationPropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                UrlNamePropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(UrlNamePropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{UrlNamePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                DefaultPageForLocationPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(DefaultPageForLocationPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{DefaultPageForLocationPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                FullUrlPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(FullUrlPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{FullUrlPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringArrayContentPropertyMatchesNodeProperty(
                RedirectLocationsPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(RedirectLocationsPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            return matched ? (true, "") : (false, $"{RedirectLocationsPropertyName} did not validate: {failureReason}");
        }
    }
#pragma warning restore S1481 // Unused local variables should be removed
}
