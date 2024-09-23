using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IJobProfileCacheRefresh
    {
        Task RefreshAllJobProfileContent(Processing processing);
    }
}
