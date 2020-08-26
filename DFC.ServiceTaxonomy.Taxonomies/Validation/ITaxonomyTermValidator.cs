using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Taxonomies.Validation
{
    public interface ITaxonomyTermValidator
    {
        Task<bool> Validate(JObject term, JObject taxonomy);
        string ErrorMessage { get; }
    }
}
