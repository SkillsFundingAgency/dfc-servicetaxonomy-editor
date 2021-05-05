using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Media.Services
{
    public interface ITokenService
    {
        Task<string> GetAccessToken();
    }
}
