using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.PageLocation.Constants;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.PageLocation.GraphSyncers
{
    public class PageLocationPartGraphSyncer : ContentPartGraphSyncer
    {
        private readonly IPageLocationClonePropertyGenerator _generator;
        private readonly IContentItemsService _contentItemsService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILogger<PageLocationPartGraphSyncer> _logger;

        public PageLocationPartGraphSyncer(IPageLocationClonePropertyGenerator generator, IContentItemsService contentItemsService, IContentDefinitionManager contentDefinitionManager, ILogger<PageLocationPartGraphSyncer> logger)
        {
            _generator = generator;
            _contentItemsService = contentItemsService;
            _contentDefinitionManager = contentDefinitionManager;
            _logger= logger;
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
            _logger.LogInformation($"AddSyncComponents");
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_pageLocationPropertyNameTransform);

            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(UrlNamePropertyName), content, UrlNamePropertyName);
            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(FullUrlPropertyName), content, FullUrlPropertyName);

            var settings = context.ContentTypePartDefinition.GetSettings<PageLocationPartSettings>();

            //TODO : if this setting changes, do we need to also check/remove these properties from the node?
            if (settings.DisplayRedirectLocationsAndDefaultPageForLocation)
            {
                _logger.LogInformation($"DisplayRedirectLocationsAndDefaultPageForLocation {settings.DisplayRedirectLocationsAndDefaultPageForLocation}");
                context.MergeNodeCommand.AddProperty<bool>(await context.SyncNameProvider.PropertyName(DefaultPageForLocationPropertyName), content, DefaultPageForLocationPropertyName);

                _logger.LogInformation($"RedirectLocationsPropertyName {context.SyncNameProvider.PropertyName(RedirectLocationsPropertyName)}");

                context.MergeNodeCommand.AddArrayPropertyFromMultilineString(
                    await context.SyncNameProvider.PropertyName(RedirectLocationsPropertyName), content,
                    RedirectLocationsPropertyName);
            }
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
                context.NodeWithRelationships.SourceNode!);

            _logger.LogInformation($"ValidateSyncComponent UrlNamePropertyName {UrlNamePropertyName} ");

            if (!matched)
                return (false, $"{UrlNamePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                FullUrlPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(FullUrlPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
            {
                _logger.LogInformation($"FullUrlPropertyName {FullUrlPropertyName} did not validate");
                return (false, $"{FullUrlPropertyName} did not validate: {failureReason}");
            }

            var settings = context.ContentTypePartDefinition.GetSettings<PageLocationPartSettings>();

            if (settings.DisplayRedirectLocationsAndDefaultPageForLocation)
            {
                (matched, failureReason) = context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                    DefaultPageForLocationPropertyName,
                    content,
                    await context.SyncNameProvider.PropertyName(DefaultPageForLocationPropertyName),
                    context.NodeWithRelationships.SourceNode!);

                if (!matched)
                {
                    _logger.LogInformation($"DefaultPageForLocationPropertyName {DefaultPageForLocationPropertyName} did not validate");
                    return (false, $"{DefaultPageForLocationPropertyName} did not validate: {failureReason}");

                }

                (matched, failureReason) = context.GraphValidationHelper.ContentMultilineStringPropertyMatchesNodeProperty(
                    RedirectLocationsPropertyName,
                    content,
                    await context.SyncNameProvider.PropertyName(RedirectLocationsPropertyName),
                    context.NodeWithRelationships.SourceNode!);

                return matched ? (true, "") : (false, $"{RedirectLocationsPropertyName} did not validate: {failureReason}");
            }

            return (true, "");
        }

        public override async Task MutateOnClone(JObject content, ICloneContext context)
        {
            string? urlName = (string?)content[nameof(PageLocationPart.UrlName)];
            string? fullUrl = (string?)content[nameof(PageLocationPart.FullUrl)];

            if (string.IsNullOrWhiteSpace(urlName) || string.IsNullOrWhiteSpace(fullUrl))
            {
                _logger.LogInformation($"MutateOnClone Cannot mutate {nameof(PageLocationPart)}");
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
