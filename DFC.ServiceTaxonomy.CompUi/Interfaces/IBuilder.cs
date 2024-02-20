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
        Task<bool> InvalidatePageNodeAsync(Processing processing);
        Task<bool> InvalidateAdditionalPageNodesAsync(Processing processing);
        Task<bool> InvalidateSharedContentAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityFilteringQuestionAsync();
        Task<bool> InvalidateDysacPersonalityQuestionSetAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityShortQuestionAsync();
        Task<bool> InvalidateDysacPersonalityTraitAsync();
        Task<bool> InvalidateDysacJobProfileOverviewAsync(Processing processing);
        Task<bool> InvalidateTriageToolFiltersAsync(Processing processing);
        Task<bool> InvalidateAdditionalContentItemIdsAsync(Processing processing, IEnumerable<RelatedItems> data);
        //Task<string> GetSharedContentNodeIdAsync(Processing processing, IEnumerable<NodeItem> data);
        //Task<string> GetPageNodeIdAsync(Processing processing);
        Task<bool> InvalidateJobProfileCategoriesAsync(Processing processing);
        Task<bool> InvalidateJobProfileCategoryAsync();
        Task<bool> InvalidateJobProfileAsync(Processing processing);
        //Task<bool> InvalidateDysacJobProfileCategoryAsync(Processing processing);
        //Task<bool> InvalidateNodeAsync(Processing processing);
    }
}
