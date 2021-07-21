using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Content.Services.Interface
{
    public interface ITokenService
    {
        Task<string?> GetAccessToken();
    }
}
