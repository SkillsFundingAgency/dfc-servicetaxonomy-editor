using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Media.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecrectAsync(string keyVaultKey);
    }
}
