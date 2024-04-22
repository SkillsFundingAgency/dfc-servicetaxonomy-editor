using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.AppRegistry
{
    public interface IPageLocationUpdater
    {
        Task<ContentPageModel?> GetPageById(string filter);

        Task<ContentPageModel> UpdatePages(string nodeId, List<string> locations, string filter);

        Task<ContentPageModel> DeletePages(string nodeId, string filter);

    }
}
