using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface ISynonymService
    {
        Task<IEnumerable<string>> GetSynonymsAsync(string node);
    }
}
