using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Taxonomies.Validation
{
    public interface ITaxonomyTermUpdateValidator
    {
        Task<bool> Validate(JObject term, JObject taxonomy);
        string ErrorMessage { get; }
    }
}
