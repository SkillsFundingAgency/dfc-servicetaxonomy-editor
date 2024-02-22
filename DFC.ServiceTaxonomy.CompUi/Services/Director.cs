using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Services
{
    public class Director : IDirector
    {
        private IBuilder? _builder;

        public IBuilder Builder
        {
            set { _builder = value; }
        }

        public async Task ProcessSharedContentAsync(Processing processing)
        {
            await _builder.InvalidateAdditionalPageNodesAsync(processing);

            var data = await _builder.GetContentItemsByLikeQueryAsync(nameof(PublishedContentTypes.Page), processing.ContentItemId);

            if (data != null)
            {
                foreach (var item in data)
                {
                    await _builder.InvalidatePageNodeAsync(item.Content, processing.EventType);
                }
            }

            await _builder.InvalidateSharedContentAsync(processing);
        }

        public async Task ProcessPageAsync(Processing processing)
        {
            await _builder.InvalidateAdditionalPageNodesAsync(processing);
            await _builder.InvalidatePageNodeAsync(processing.Content, processing.EventType);
            await _builder.InvalidateTriageToolFiltersAsync(processing);
        }

        public async Task ProcessBannerAsync(Processing processing)
        {
            await _builder.InvalidatePageBannerAsync(processing);
        }

        public async Task ProcessTriageToolFilterAsync(Processing processing)
        {
            await _builder.InvalidateTriageToolFiltersAsync(processing);
        }

        public async Task ProcessPagebannerAsync(Processing processing)
        {
            await _builder.InvalidatePageBannerAsync(processing);
        }

        public async Task ProcessJobProfileCategoryAsync(Processing processing)
        {
            await _builder.InvalidateJobProfileCategoryAsync();
            await _builder.InvalidateDysacPersonalityTraitAsync();
            await _builder.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing);
        }

        public async Task ProcessJobProfileAsync(Processing processing)
        {
            await _builder.InvalidateJobProfileCategoryAsync();
            await _builder.InvalidateDysacJobProfileOverviewAsync(processing);
            await _builder.InvalidateJobProfileAsync(processing);
        }

        public async Task ProcessPersonalityFilteringQuestionAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            await _builder.InvalidateDysacPersonalityFilteringQuestionAsync();
        }

        public async Task ProcessPersonalityQuestionSetAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            await _builder.InvalidateDysacPersonalityQuestionSetAsync(processing);
        }

        public async Task ProcessPersonalityShortQuestionAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            await _builder.InvalidateDysacPersonalityShortQuestionAsync();
        }

        public async Task ProcessPersonalityTraitAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            await _builder.InvalidateDysacPersonalityTraitAsync();
        }

        public async Task ProcessSOCCodeAsync(Processing processing) => throw new NotImplementedException();

        public async Task ProcessSOCSkillsMatrixAsync(Processing processing)
        {
            await _builder.InvalidateDysacPersonalityFilteringQuestionAsync();
            await _builder.InvalidateJobProfileCategoryAsync();
        }

        public async Task ProcessDynamicTitlePrefixAsync(Processing processing) => throw new NotImplementedException();

        public async Task ProcessWorkingPatternsAsync(Processing processing)
        {
            await _builder.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing);
        }

        public async Task ProcessWorkingPatternDetailAsync(Processing processing)
        {
            await _builder.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing);
        }

        public async Task ProcessWorkingHoursDetailAsync(Processing processing)
        {
            await _builder.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing);
        }
    }
}
