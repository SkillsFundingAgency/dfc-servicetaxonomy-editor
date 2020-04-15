using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface ISynonymService
    {
        IEnumerable<string> GetSynonyms(string node);
    }
}
