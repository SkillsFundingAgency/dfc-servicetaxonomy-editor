using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.Settings;
using DFC.ServiceTaxonomy.Taxonomies.ViewModels;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.Taxonomies.Drivers
{
    public class TaxonomyContentsAdminListDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        private const string LevelPadding = "\xA0\xA0";

        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IStringLocalizer S;

        public TaxonomyContentsAdminListDisplayDriver(
            ISiteService siteService,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<TaxonomyContentsAdminListDisplayDriver> stringLocalizer)
        {
            _siteService = siteService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            S = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();

            if (!settings.TaxonomyContentItemIds.Any())
            {
                return null;
            }

            var taxonomyContentItemIds = settings.TaxonomyContentItemIds;
            if (!String.IsNullOrEmpty(model.SelectedContentType))
            {
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(model.SelectedContentType);
                var fieldDefinitions = contentTypeDefinition
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)));
                var fieldTaxonomyContentItemIds = fieldDefinitions.Select(x => x.GetSettings<TaxonomyFieldSettings>().TaxonomyContentItemId);
                taxonomyContentItemIds = taxonomyContentItemIds.Intersect(fieldTaxonomyContentItemIds).ToArray();

                if (!taxonomyContentItemIds.Any())
                {
                    return null;
                }
            }

            var results = new List<IDisplayResult>();
            var taxonomies = await _contentManager.GetAsync(taxonomyContentItemIds);

            var position = 5;
            foreach (var taxonomy in taxonomies)
            {
                results.Add(
                    Initialize<TaxonomyContentsAdminFilterViewModel>("ContentsAdminListTaxonomyFilter", m =>
                    {
                        var termEntries = new List<FilterTermEntry>();
                        PopulateTermEntries(termEntries, taxonomy.As<TaxonomyPart>().Terms, 0);
                        var terms = new List<SelectListItem>
                            {
                                new SelectListItem() { Text = S["Clear filter"], Value = ""  },
                                new SelectListItem() { Text = S["Show all"], Value = "Taxonomy:" + taxonomy.ContentItemId }
                            };

                        foreach (var term in termEntries)
                        {
                            using var sb = StringBuilderPool.GetInstance();
                            for (var l = 0; l < term.Level; l++)
                            {
                                sb.Builder.Insert(0, LevelPadding);
                            }
                            sb.Builder.Append(term.DisplayText);
                            var item = new SelectListItem() { Text = sb.Builder.ToString(), Value = "Term:" + term.ContentItemId };
                            terms.Add(item);
                        }

                        m.DisplayText = taxonomy.DisplayText;
                        m.Taxonomies = terms;
                    })
                    .Location("Actions:40." + position.ToString())
                    .Prefix("Taxonomy" + taxonomy.ContentItemId)
                );

                position += 5;
            }

            if (results.Any())
            {
                return Combine(results);
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();
            foreach (var contentItemId in settings.TaxonomyContentItemIds)
            {
                var viewModel = new TaxonomyContentsAdminFilterViewModel();
                if (await updater.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId))
                {
                    if (!String.IsNullOrEmpty(viewModel.SelectedContentItemId))
                    {
                        model.RouteValues.TryAdd("Taxonomy" + contentItemId + ".SelectedContentItemId", viewModel.SelectedContentItemId);
                    }
                }
            }

            return await EditAsync(model, updater);
        }

        private static void PopulateTermEntries(List<FilterTermEntry> termEntries, IEnumerable<ContentItem> contentItems, int level)
        {
            foreach (var contentItem in contentItems)
            {
                var children = Array.Empty<ContentItem>();

                if (contentItem.Content.Terms is JArray termsArray)
                {
                    children = termsArray.ToObject<ContentItem[]>();
                }

                var termEntry = new FilterTermEntry
                {
                    DisplayText = contentItem.DisplayText,
                    ContentItemId = contentItem.ContentItemId,
                    Level = level
                };

                termEntries.Add(termEntry);

                if (children.Length > 0)
                {
                    PopulateTermEntries(termEntries, children, level + 1);
                }
            }
        }
    }

    public class FilterTermEntry
    {
        public string DisplayText { get; set; }
        public string ContentItemId { get; set; }
        public int Level { get; set; }
    }
}
