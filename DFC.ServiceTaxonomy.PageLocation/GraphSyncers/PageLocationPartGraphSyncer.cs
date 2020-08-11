using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.PageLocation.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.PageLocation.GraphSyncers
{
    public class PageLocationPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(PageLocationPart);

        private static readonly Func<string, string> _pageLocationPropertyNameTransform = n => $"pagelocation_{n}";

        private const string
            UrlNamePropertyName = "UrlName",
            DefaultPageForLocationPropertyName = "DefaultPageForLocation",
            FullUrlPropertyName = "FullUrl",
            RedirectLocationsPropertyName = "RedirectLocations";

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            using var _ = context.GraphSyncHelper.PushPropertyNameTransform(_pageLocationPropertyNameTransform);

            context.MergeNodeCommand.AddProperty<string>(await context.GraphSyncHelper.PropertyName(UrlNamePropertyName), content, UrlNamePropertyName);
            context.MergeNodeCommand.AddProperty<bool>(await context.GraphSyncHelper.PropertyName(DefaultPageForLocationPropertyName), content, DefaultPageForLocationPropertyName);
            context.MergeNodeCommand.AddProperty<string>(await context.GraphSyncHelper.PropertyName(FullUrlPropertyName), content, FullUrlPropertyName);

            context.MergeNodeCommand.AddArrayPropertyFromMultilineString(
                await context.GraphSyncHelper.PropertyName(RedirectLocationsPropertyName), content,
                RedirectLocationsPropertyName);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
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

            (matched, failureReason) = context.GraphValidationHelper.ContentMultilineStringPropertyMatchesNodeProperty(
                RedirectLocationsPropertyName,
                content,
                await context.GraphSyncHelper.PropertyName(RedirectLocationsPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            return matched ? (true, "") : (false, $"{RedirectLocationsPropertyName} did not validate: {failureReason}");
        }
    }
}
