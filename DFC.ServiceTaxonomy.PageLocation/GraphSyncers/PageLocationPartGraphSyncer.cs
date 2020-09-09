using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.Services;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.PageLocation.GraphSyncers
{
    public class PageLocationPartGraphSyncer : ContentPartGraphSyncer
    {
        private readonly IPageLocationClonePropertyGenerator _generator;
        private readonly IContentItemsService _contentItemsService;

        public PageLocationPartGraphSyncer(IPageLocationClonePropertyGenerator generator, IContentItemsService contentItemsService)
        {
            _generator = generator;
            _contentItemsService = contentItemsService;
        }

        public override string PartName => nameof(PageLocationPart);

        private static readonly Func<string, string> _pageLocationPropertyNameTransform = n => $"pagelocation_{n}";

        private const string
            UrlNamePropertyName = "UrlName",
            DefaultPageForLocationPropertyName = "DefaultPageForLocation",
            FullUrlPropertyName = "FullUrl",
            RedirectLocationsPropertyName = "RedirectLocations";

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_pageLocationPropertyNameTransform);

            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(UrlNamePropertyName), content, UrlNamePropertyName);
            context.MergeNodeCommand.AddProperty<bool>(await context.SyncNameProvider.PropertyName(DefaultPageForLocationPropertyName), content, DefaultPageForLocationPropertyName);
            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(FullUrlPropertyName), content, FullUrlPropertyName);

            context.MergeNodeCommand.AddArrayPropertyFromMultilineString(
                await context.SyncNameProvider.PropertyName(RedirectLocationsPropertyName), content,
                RedirectLocationsPropertyName);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_pageLocationPropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                UrlNamePropertyName,
                content,
                await context.SyncNameProvider.PropertyName(UrlNamePropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{UrlNamePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                DefaultPageForLocationPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(DefaultPageForLocationPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{DefaultPageForLocationPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                FullUrlPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(FullUrlPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            if (!matched)
                return (false, $"{FullUrlPropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.ContentMultilineStringPropertyMatchesNodeProperty(
                RedirectLocationsPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(RedirectLocationsPropertyName),
                context.NodeWithOutgoingRelationships.SourceNode);

            return matched ? (true, "") : (false, $"{RedirectLocationsPropertyName} did not validate: {failureReason}");
        }

        public override async Task MutateOnClone(JObject content, ICloneContext context)
        {
            string? urlName = (string?)content[nameof(PageLocationPart.UrlName)];
            string? fullUrl = (string?)content[nameof(PageLocationPart.FullUrl)];

            if (string.IsNullOrWhiteSpace(urlName) || string.IsNullOrWhiteSpace(fullUrl))
            {
                throw new InvalidOperationException($"Cannot mutate {nameof(PageLocationPart)} if {nameof(PageLocationPart.UrlName)} or {nameof(PageLocationPart.FullUrl)} are missing.");
            }

            List<ContentItem> pages = await _contentItemsService.GetActive(ContentTypes.Page);

            string urlSearchFragment = _generator.GenerateUrlSearchFragment(fullUrl);

            IEnumerable<ContentItem> existingClones = pages.Where(x => ((string)x.Content[nameof(PageLocationPart)][nameof(PageLocationPart.FullUrl)]).Contains(urlSearchFragment));

            var result = _generator.GenerateClonedPageLocationProperties(urlName, fullUrl, existingClones);

            content[nameof(PageLocationPart.UrlName)] = result.UrlName;
            content[nameof(PageLocationPart.FullUrl)] = result.FullUrl;
            content[nameof(PageLocationPart.DefaultPageForLocation)] = false;
            content[nameof(PageLocationPart.RedirectLocations)] = null;
        }
    }
}
