using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Content.Services.Interface
{
    public interface ICdnService
    {
        Task<bool> PurgeContentAsync(IList<string> contentPaths);
    }
}
