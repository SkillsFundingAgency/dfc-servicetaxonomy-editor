using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Taxonomies.Validation
{
    public interface ITaxonomyTermValidator
    {
        Task<(bool, string)> ValidateCreate(JsonObject term, JsonObject taxonomy);
        Task<(bool, string)> ValidateUpdate(JsonObject term, JsonObject taxonomy);
        Task<(bool, string)> ValidateDelete(JsonObject term, JsonObject taxonomy);
    }
}
