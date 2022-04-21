using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Services.Interface
{
    public interface ISynonymService
    {
        Task<IEnumerable<string>> GetSynonymsAsync(string node);
    }
}
