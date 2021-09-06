using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public interface IContentNameService
    {
        Task<string> GetContentNameAsync(string contentItemId);
    }
}
