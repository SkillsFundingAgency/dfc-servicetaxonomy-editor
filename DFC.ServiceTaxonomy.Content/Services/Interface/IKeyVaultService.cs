using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Content.Services.Interface
{
    public interface IKeyVaultService
    {
        Task<string?> GetSecrectAsync(string keyVaultKey);
    }
}
