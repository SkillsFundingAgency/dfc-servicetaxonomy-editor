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
        Task<bool> InvalidatePageNodeAsync(string content, ProcessingEvents processingEvents);
        Task InvalidateAdditionalPageNodesAsync(Processing processing);
        Task<bool> InvalidateSharedContentAsync(Processing processing);
        Task<bool> InvalidateDysacPersonalityFilteringQuestionAsync();
        Task<bool> InvalidateDysacPersonalityQuestionSetAsync();
        Task<bool> InvalidateDysacPersonalityShortQuestionAsync();
        Task<bool> InvalidateDysacPersonalityTraitAsync();
        Task InvalidateDysacJobProfileOverviewAsync(Processing processing);
        Task InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(Processing processing);
        Task<bool> InvalidateTriageToolFiltersAsync(Processing processing);
        Task<bool> InvalidateAdditionalContentItemIdsAsync(Processing processing, IEnumerable<RelatedItems> data);
        Task<bool> InvalidateJobProfileCategoriesAsync(Processing processing);
        Task<bool> InvalidateJobProfileCategoryAsync();
        Task<bool> InvalidateJobProfileAsync(Processing processing);
    }
}
