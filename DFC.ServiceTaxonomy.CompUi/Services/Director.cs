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
            //var data = await _builder.GetDataAsync(processing);  //Actually no need to get the data as all the required data can be pulled from the Content and in the case of "Remove" will need to pull this from there anyway.
            //var success = await _builder.InvalidateSharedContentAsync(processing, data);
            var success = await _builder.InvalidateSharedContentAsync(processing);
            return success;
        }

        public async Task<bool> ProcessPageAsync(Processing processing)
        {
            //If we already have the content is there any point querying the database.
            //var data = await _builder.GetDataAsync(processing);
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
            var success = await _builder.InvalidateJobProfileCategory();
            success = await _builder.InvalidateJobProfileOverview(processing);
            success = await _builder.InvalidateJobProfile(processing);
            return success;
        }

        public async Task<bool> ProcessJobProfileAsync(Processing processing)
        {
            var success = await _builder.InvalidateJobProfileCategories(processing);
            success = await _builder.InvalidateJobProfileOverview(processing);
            success = await _builder.InvalidateJobProfile(processing);
            return success;
        }

        public async Task<bool> ProcessTriageToolOptionAsync(Processing processing) => throw new NotImplementedException();

        public async Task<bool> ProcessPersonalityFilteringQuestionAsync(Processing processing)
        {
            //Issues -> speak to Zl can't create question as I don't have a SOC Skills Matrix

            //Data dependencies:
            //PersonalityFilteringQuestion
            //SOCSkillsMatrix

            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            var success = await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            success = await _builder.InvalidatePersonalityFilteringQuestionAsync(processing);
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
            success = await _builder.InvalidatePersonalityQuestionSetAsync(processing);
            return success;
        }

        public async Task<bool> ProcessPersonalityShortQuestionAsync(Processing processing)
        {
            var data = await _builder.GetRelatedContentItemIdsAsync(processing);
            var success = await _builder.InvalidateAdditionalContentItemIdsAsync(processing, data);
            success = await _builder.InvalidatePersonalityShortQuestionAsync(processing);
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
            success = await _builder.InvalidatePersonalityTraitAsync(processing);
            return success;
        }

        public async Task<bool> ProcessSOCCodeAsync(Processing processing) => throw new NotImplementedException();

        public async Task<bool> ProcessSOCSkillsMatrixAsync(Processing processing)
        {
            var success = true;
            success = await _builder.InvalidatePersonalityFilteringQuestionAsync(processing);
            //TODO
            //Invalidates JobProfileCategory
            return success;
        }


        public async Task<bool> ProcessDynamicTitlePrefixAsync(Processing processing) => throw new NotImplementedException();
    }
}
