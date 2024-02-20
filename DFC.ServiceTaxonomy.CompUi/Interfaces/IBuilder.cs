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
        //Task<bool> InvalidateSharedContentAsync(Processing processing, IEnumerable<NodeItem> data);

        Task<bool> InvalidatePersonalityFilteringQuestionAsync(Processing processing);

        Task<bool> InvalidatePersonalityQuestionSetAsync(Processing processing);

        Task<bool> InvalidatePersonalityShortQuestionAsync(Processing processing);

        Task<bool> InvalidatePersonalityTraitAsync(Processing processing);

        //Task<bool> InvalidateJobProfileCategoryAsync(Processing processing);

        Task<bool> InvalidateAdditionalContentItemIdsAsync(Processing processing, IEnumerable<RelatedItems> data);

        //Task<string> GetSharedContentNodeIdAsync(Processing processing, IEnumerable<NodeItem> data);

        //Task<string> GetPageNodeIdAsync(Processing processing);

        Task<bool> InvalidateJobProfileCategories(Processing processing);
        Task<bool> InvalidateJobProfileCategory();
        Task<bool> InvalidateJobProfileOverview(Processing processing);
        Task<bool> InvalidateJobProfile(Processing processing);

        //Task<bool> InvalidateNodeAsync(Processing processing);
    }
}
