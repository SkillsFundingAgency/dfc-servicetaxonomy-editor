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

        public async Task<bool> ProcessSharedContentAsync(Processing processing)
        {
            var success = await _builder.InvalidateSharedContentAsync(processing);
            return success;
        }

        public async Task<bool> ProcessPageAsync(Processing processing)
        {
            var success = await _builder.InvalidateAdditionalPageNodesAsync(processing);
            success = await _builder.InvalidatePageNodeAsync(processing);
            success = await _builder.InvalidateTriageToolFiltersAsync(processing);
            return success;
        }

        public async Task<bool> ProcessBannerAsync(Processing processing)
        {
            var success = await _builder.InvalidatePageBannerAsync(processing);
            return true;
        }

        public async Task<bool> ProcessTriageToolFilterAsync(Processing processing)
        {
            var success = await _builder.InvalidateTriageToolFiltersAsync(processing);
            return success;
        }

        public async Task<bool> ProcessPagebannerAsync(Processing processing)
        {
            var success = await _builder.InvalidatePageBannerAsync(processing);
            return success;
        }

        public async Task<bool> ProcessJobProfileCategoryAsync(Processing processing)
        {
            var success = await _builder.InvalidateJobProfileCategoryAsync();
            success = await _builder.InvalidateDysacJobProfileOverviewAsync(processing);
            //success = await _builder.InvalidateDysacJobProfileCategoryAsync(processing);
            success = await _builder.InvalidateJobProfileAsync(processing);
            success = await _builder.InvalidateDysacPersonalityTraitAsync();
            return success;
        }

        public async Task<bool> ProcessJobProfileAsync(Processing processing)
        {
            var success = await _builder.InvalidateJobProfileCategoriesAsync(processing);
            success = await _builder.InvalidateDysacJobProfileOverviewAsync(processing);
            success = await _builder.InvalidateJobProfileAsync(processing);
            return success;
        }

        public async Task<bool> ProcessPersonalityFilteringQuestionAsync(Processing processing)
        {
            //Data dependencies:
            //PersonalityFilteringQuestion
            //SOCSkillsMatrix

            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            var success = await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            success = await _builder.InvalidateDysacPersonalityFilteringQuestionAsync();
            return success;
        }

        public async Task<bool> ProcessPersonalityQuestionSetAsync(Processing processing)
        {
            //Data dependencies:
            //Personality Question Set
            //PersonalityShortQuestion
            //Personality Trait
            //JobProfileCategory

            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            var success = await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            success = await _builder.InvalidateDysacPersonalityQuestionSetAsync(processing);
            return success;
        }

        public async Task<bool> ProcessPersonalityShortQuestionAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            var success = await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            success = await _builder.InvalidateDysacPersonalityShortQuestionAsync();
            return success;
        }

        public async Task<bool> ProcessPersonalityTraitAsync(Processing processing)
        {
            //Data dependencies:
            //Personality Trait
            //JobProfileCategory
            //JobProfile

            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            var success = await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            success = await _builder.InvalidateDysacPersonalityTraitAsync();
            return success;
        }

        public async Task<bool> ProcessSOCCodeAsync(Processing processing) => throw new NotImplementedException();

        public async Task<bool> ProcessSOCSkillsMatrixAsync(Processing processing)
        {
            var success = await _builder.InvalidateDysacPersonalityFilteringQuestionAsync();
            success = await _builder.InvalidateJobProfileCategoryAsync();
            return success;
        }

        public async Task<bool> ProcessDynamicTitlePrefixAsync(Processing processing) => throw new NotImplementedException();
    }
}
