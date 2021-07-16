using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Media.Services
{
    public interface ICdnService
    {
        Task<bool> PurgeContentAsync(IList<string> contentPaths);
    }
}
