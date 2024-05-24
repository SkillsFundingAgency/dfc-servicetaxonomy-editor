using System.Diagnostics;
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

            var data = await _builder.GetContentItemsByLikeQueryAsync(nameof(ContentTypes.Page), processing.ContentItemId);

            if (data != null)
            {
                foreach (var item in data)
                {
                    await _builder.InvalidatePageNodeAsync(item.Content, processing);
                }
            }

            await _builder.InvalidateSharedContentAsync(processing);
        }

        public async Task ProcessPageAsync(Processing processing)
        {
            await _builder.InvalidateAdditionalPageNodesAsync(processing);
            await _builder.InvalidatePageNodeAsync(processing.CurrentContent, processing);
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
            await _builder.InvalidateJobProfileCategoryAsync(processing);
            await _builder.InvalidateDysacPersonalityTraitAsync(processing);
            await _builder.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing);
        }

        public async Task ProcessJobProfileAsync(Processing processing)
        {
            await Task.WhenAll(_builder.InvalidateAllJobProfileContentAsync(processing));

            //if (processing.EventType == ProcessingEvents.Published)
            //{
            //    Task.WaitAll(_builder.RefreshAllJobProfileContent(processing));
            //}
        }

        public async Task ProcessPersonalityFilteringQuestionAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            await _builder.InvalidateDysacPersonalityFilteringQuestionAsync(processing);
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
            await _builder.InvalidateDysacPersonalityShortQuestionAsync(processing);
        }

        public async Task ProcessPersonalityTraitAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            await _builder.InvalidateDysacPersonalityTraitAsync(processing);
        }

        public async Task ProcessSOCCodeAsync(Processing processing) => throw new NotImplementedException();

        public async Task ProcessSOCSkillsMatrixAsync(Processing processing)
        {
            await _builder.InvalidateDysacPersonalityFilteringQuestionAsync(processing);
            await _builder.InvalidateJobProfileCategoryAsync(processing);
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

        public async Task ProcessSkillsAsync(Processing processing)
        {
            await _builder.InvalidateSkillsAsync(processing);
        }
    }
}
