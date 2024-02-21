using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IDirector
    {
        public IBuilder Builder { set; }
        Task<bool> ProcessSharedContentAsync(Processing processing);
        Task<bool> ProcessPageAsync(Processing processing);
        Task<bool> ProcessBannerAsync(Processing processing);
        Task<bool> ProcessJobProfileCategoryAsync(Processing processing);
        Task<bool> ProcessJobProfileAsync(Processing processing);
        Task<bool> ProcessPagebannerAsync(Processing processing);
        Task<bool> ProcessTriageToolFilterAsync(Processing processing);
        Task<bool> ProcessPersonalityFilteringQuestionAsync(Processing processing);
        Task<bool> ProcessPersonalityQuestionSetAsync(Processing processing);
        Task<bool> ProcessPersonalityShortQuestionAsync(Processing processing);
        Task<bool> ProcessPersonalityTraitAsync(Processing processing);
        Task<bool> ProcessSOCCodeAsync(Processing processing);
        Task<bool> ProcessSOCSkillsMatrixAsync(Processing processing);
        Task<bool> ProcessDynamicTitlePrefixAsync(Processing processing);
        Task ProcessWorkingPatternsAsync(Processing processing);
        Task ProcessWorkingPatternDetailAsync(Processing processing);
        Task ProcessWorkingHoursDetailAsync(Processing processing);

    }
}
