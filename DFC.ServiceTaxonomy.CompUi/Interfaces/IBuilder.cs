using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IBuilder
    {
        Task<bool> InvalidateBannerAsync(Processing processing);
        Task<bool> InvalidatePageBannerAsync(Processing processing);
        Task<IEnumerable<NodeItem>> GetDataAsync(Processing processing);
        Task<IEnumerable<RelatedItems>?> GetRelatedContentItemIdsAsync(Processing processing);
        Task<IEnumerable<ContentItems>?> GetContentItemsByLikeQueryAsync(string contentType, string queryIds);
        Task InvalidatePageNodeAsync(string content, Processing processing);
        Task InvalidateAdditionalPageNodesAsync(Processing processing);
        Task<bool> InvalidateSharedContentAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityFilteringQuestionAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityQuestionSetAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityShortQuestionAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityTraitAsync(Processing processing);
        Task InvalidateDysacJobProfileOverviewAsync(Processing processing);
        Task InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(Processing processing);
        Task<bool> InvalidateTriageToolFiltersAsync(Processing processing);
        Task<bool> InvalidateAdditionalContentItemIdsAsync(Processing processing, IEnumerable<RelatedItems> data);
        Task<bool> InvalidateJobProfileCategoriesAsync(Processing processing);
        Task<bool> InvalidateJobProfileCategoryAsync(Processing processing);
        Task InvalidateJobProfileAsync(Processing processing);
        Task InvalidateJobProfileSkillsAsync(Processing processing);
        Task InvalidateJobProfileRelatedCareersAsync(Processing processing);
        Task InvalidateJobProfileOverviewAsync(Processing processing);
        Task InvalidateJobProfileWhatYoullDoAsync(Processing processing);
        Task InvalidateJobProfileHowToBecomeAsync(Processing processing);
        Task InvalidateJobProfileVideoAsync(Processing processing);
    }
}
