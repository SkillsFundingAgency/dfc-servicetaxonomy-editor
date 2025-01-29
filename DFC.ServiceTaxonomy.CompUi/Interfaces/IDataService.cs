using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IDataService
    {
        Task<IEnumerable<RelatedContentData>> GetRelatedContentDataByContentItemIdAndPage(Processing processing);

        Task<IEnumerable<RelatedContentData>> GetRelatedContentDataByContentItemId(Processing processing);
    }
}
