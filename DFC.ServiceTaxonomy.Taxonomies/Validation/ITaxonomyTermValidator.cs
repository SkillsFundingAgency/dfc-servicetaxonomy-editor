using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Taxonomies.Validation
{
    public interface ITaxonomyTermValidator
    {
        Task<(bool, string)> ValidateCreate(JObject term, JObject taxonomy);
        Task<(bool, string)> ValidateUpdate(JObject term, JObject taxonomy);
        Task<(bool, string)> ValidateDelete(JObject term, JObject taxonomy);
    }
}
