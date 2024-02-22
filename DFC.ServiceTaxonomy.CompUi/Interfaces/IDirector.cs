using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IDirector
    {
        public IBuilder Builder { set; }
        Task ProcessSharedContentAsync(Processing processing);
        Task ProcessPageAsync(Processing processing);
        Task ProcessBannerAsync(Processing processing);
        Task ProcessJobProfileCategoryAsync(Processing processing);
        Task ProcessJobProfileAsync(Processing processing);
        Task ProcessPagebannerAsync(Processing processing);
        Task ProcessTriageToolFilterAsync(Processing processing);
        Task ProcessPersonalityFilteringQuestionAsync(Processing processing);
        Task ProcessPersonalityQuestionSetAsync(Processing processing);
        Task ProcessPersonalityShortQuestionAsync(Processing processing);
        Task ProcessPersonalityTraitAsync(Processing processing);
        Task ProcessSOCCodeAsync(Processing processing);
        Task ProcessSOCSkillsMatrixAsync(Processing processing);
        Task ProcessDynamicTitlePrefixAsync(Processing processing);
        Task ProcessWorkingPatternsAsync(Processing processing);
        Task ProcessWorkingPatternDetailAsync(Processing processing);
        Task ProcessWorkingHoursDetailAsync(Processing processing);

    }
}
