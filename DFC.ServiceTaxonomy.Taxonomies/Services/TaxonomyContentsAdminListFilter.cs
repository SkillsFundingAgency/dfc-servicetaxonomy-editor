using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Indexing;
using DFC.ServiceTaxonomy.Taxonomies.Settings;
using DFC.ServiceTaxonomy.Taxonomies.ViewModels;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using YesSql;

namespace DFC.ServiceTaxonomy.Taxonomies.Services
{
    public class TaxonomyContentsAdminListFilter : IContentsAdminListFilter
    {
        private readonly ISiteService _siteService;
        private readonly ILogger _logger;

        public TaxonomyContentsAdminListFilter(ISiteService siteService, ILogger logger)
        {
            _siteService = siteService;
            _logger = logger;
        }

        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();
            _logger.LogInformation($"FilterAsync: query {query} ");
            foreach (var contentItemId in settings.TaxonomyContentItemIds)
            {
                var viewModel = new TaxonomyContentsAdminFilterViewModel();
                if (await updater.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId))
                {
                    // Show all items categorized by the taxonomy
                    if (!String.IsNullOrEmpty(viewModel.SelectedContentItemId))
                    {
                        if (viewModel.SelectedContentItemId.StartsWith("Taxonomy:", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation($"FilterAsync: Taxonomy {viewModel.SelectedContentItemId} "); 
                            viewModel.SelectedContentItemId = viewModel.SelectedContentItemId.Substring(9);
                            query.All(
                                x => query.With<TaxonomyIndex>(x => x.TaxonomyContentItemId == viewModel.SelectedContentItemId)
                            );
                        }
                        else if (viewModel.SelectedContentItemId.StartsWith("Term:", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation($"FilterAsync: Term {viewModel.SelectedContentItemId} ");
                            viewModel.SelectedContentItemId = viewModel.SelectedContentItemId.Substring(5);
                            query.All(
                                x => query.With<TaxonomyIndex>(x => x.TermContentItemId == viewModel.SelectedContentItemId)
                            );
                        }
                    }
                }
            }
        }
    }
}
