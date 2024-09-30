using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IRelatedContentItemIndexRepository
    {
        Task<IEnumerable<RelatedItems>> GetRelatedContentItemData(Processing processing);
    }
}
